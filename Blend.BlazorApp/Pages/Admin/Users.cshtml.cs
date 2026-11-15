using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Policy = "PasswordChangeComplete")]
[Authorize(Policy = BlendPermissions.ManageUsersPolicy)]
public sealed class UsersModel : PageModel
{
    private readonly IUserAdministrationService _userAdministration;
    private readonly BlendIdentityDbContext _database;
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly RoleManager<BlendIdentityRole> _roleManager;

    public UsersModel(
        IUserAdministrationService userAdministration,
        BlendIdentityDbContext database,
        UserManager<BlendIdentityUser> userManager,
        RoleManager<BlendIdentityRole> roleManager)
    {
        _userAdministration = userAdministration;
        _database = database;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    [BindProperty]
    [ValidateNever]
    public EditUserInput EditInput { get; set; } = new();

    [BindProperty]
    public MembershipFormInput MembershipInput { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public IReadOnlyList<RoleOption> AvailableRoles { get; private set; } = [];
    public IReadOnlyList<CompanyRoleOption> CompanyRoles { get; private set; } = [];
    public string? TemporaryPassword { get; private set; }
    public string? SuccessMessage { get; private set; }
    public bool ShowCreateDialog { get; private set; }
    public string? ShowEditUserId { get; private set; }
    public string? ShowMembershipUserId { get; private set; }

    public async Task OnGetAsync() => await LoadUsersAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        ShowCreateDialog = true;
        if (!Guid.TryParse(CompanyId, out var companyId) || !await CanManageCompanyAsync(companyId))
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
        }

        if (ModelState.IsValid)
        {
            var result = await _userAdministration.CreateUserAsync(
                Input.UserName,
                Input.DisplayName,
                Input.Email,
                Input.RoleId,
                companyId,
                HttpContext.RequestAborted);

            if (result.Succeeded)
            {
                TemporaryPassword = result.TemporaryPassword;
                SuccessMessage = result.IsExistingUser
                    ? "Existing user added to this company. They must sign in again for the new role to take effect."
                    : "User created. Copy the temporary password now; it will not be shown again.";
                ShowCreateDialog = false;
                Input = new CreateUserInput();
                ModelState.Clear();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
        }

        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(string id)
    {
        ShowEditUserId = id;
        ModelState.Clear();
        if (!TryValidateModel(EditInput, nameof(EditInput)))
        {
            await LoadUsersAsync();
            return Page();
        }

        var isPlatformAdmin = User.IsInRole(BlendRoles.Admin);
        var companyId = Guid.Empty;
        if (!isPlatformAdmin
            && (!Guid.TryParse(CompanyId, out companyId) || !await CanManageCompanyAsync(companyId)))
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
            await LoadUsersAsync();
            return Page();
        }
        var user = isPlatformAdmin
            ? await _userManager.FindByIdAsync(id)
            : await _database.UserCompanies
                .Where(membership => membership.UserId == id && membership.CompanyId == companyId)
                .Select(membership => membership.User)
                .SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (user is null || isPlatformAdmin && await _userManager.IsInRoleAsync(user, BlendRoles.Admin))
        {
            ModelState.AddModelError(string.Empty, "The selected user does not exist or cannot be edited here.");
            await LoadUsersAsync();
            return Page();
        }

        var role = isPlatformAdmin ? null : await _roleManager.FindByIdAsync(EditInput.RoleId);
        if (!isPlatformAdmin && (role is null || role.CompanyId != companyId || string.IsNullOrWhiteSpace(role.Name)))
        {
            ModelState.AddModelError(string.Empty, "Choose a valid role for the selected company.");
            await LoadUsersAsync();
            return Page();
        }

        EditInput.UserName = EditInput.UserName.Trim();
        EditInput.DisplayName = EditInput.DisplayName.Trim();
        EditInput.Email = string.IsNullOrWhiteSpace(EditInput.Email) ? null : EditInput.Email.Trim();
        user.UserName = EditInput.UserName;
        user.DisplayName = EditInput.DisplayName;
        user.Email = EditInput.Email;

        await using var transaction = await _database.Database.BeginTransactionAsync(HttpContext.RequestAborted);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            await transaction.RollbackAsync(HttpContext.RequestAborted);
            AddIdentityErrors(updateResult);
            _database.ChangeTracker.Clear();
            await LoadUsersAsync();
            return Page();
        }

        if (!isPlatformAdmin)
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            var companyRoleNames = await _roleManager.Roles
                .Where(candidate => candidate.CompanyId == companyId && candidate.Name != null)
                .Select(candidate => candidate.Name!)
                .ToListAsync(HttpContext.RequestAborted);
            var rolesToRemove = existingRoles.Where(companyRoleNames.Contains).ToArray();
            var removeResult = rolesToRemove.Length == 0
                ? IdentityResult.Success
                : await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                await transaction.RollbackAsync(HttpContext.RequestAborted);
                AddIdentityErrors(removeResult);
                _database.ChangeTracker.Clear();
                await LoadUsersAsync();
                return Page();
            }

            var addResult = await _userManager.AddToRoleAsync(user, role!.Name!);
            if (!addResult.Succeeded)
            {
                await transaction.RollbackAsync(HttpContext.RequestAborted);
                AddIdentityErrors(addResult);
                _database.ChangeTracker.Clear();
                await LoadUsersAsync();
                return Page();
            }
        }

        await transaction.CommitAsync(HttpContext.RequestAborted);
        SuccessMessage = isPlatformAdmin
            ? $"Profile for '{user.UserName}' updated."
            : $"User '{user.UserName}' updated. They must sign in again for role changes to take effect.";
        ShowEditUserId = null;
        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateMembershipsAsync(string id)
    {
        ShowMembershipUserId = id;
        if (!User.IsInRole(BlendRoles.Admin))
        {
            return Forbid();
        }

        ModelState.Clear();
        var user = await _userManager.FindByIdAsync(id);
        if (user is null || await _userManager.IsInRoleAsync(user, BlendRoles.Admin))
        {
            return NotFound();
        }

        var companies = await _database.Companies.AsNoTracking().ToListAsync(HttpContext.RequestAborted);
        var submittedMemberships = MembershipInput.CompanyMemberships;
        if (submittedMemberships.Count != companies.Count
            || submittedMemberships.Select(item => item.CompanyId).Distinct().Count() != companies.Count
            || submittedMemberships.Any(item => companies.All(company => company.Id != item.CompanyId)))
        {
            ModelState.AddModelError(string.Empty, "The company list is invalid. Reload the page and try again.");
        }

        var selectedMemberships = submittedMemberships.Where(item => item.HasAccess).ToArray();
        if (selectedMemberships.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "A user must belong to at least one company.");
        }

        var selectedRoleIds = selectedMemberships.Select(item => item.RoleId).ToArray();
        var selectedRoles = await _roleManager.Roles
            .Where(role => selectedRoleIds.Contains(role.Id))
            .ToListAsync(HttpContext.RequestAborted);
        if (selectedMemberships.Any(membership => string.IsNullOrWhiteSpace(membership.RoleId)
            || selectedRoles.All(role => role.Id != membership.RoleId || role.CompanyId != membership.CompanyId || string.IsNullOrWhiteSpace(role.Name))))
        {
            ModelState.AddModelError(string.Empty, "Choose a valid role for each selected company.");
        }

        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var rolesByCompany = selectedRoles.ToDictionary(role => role.CompanyId!.Value, role => role.Name!);
        var allCompanyIds = companies.Select(company => company.Id).ToArray();
        var allCompanyRoles = await _roleManager.Roles
            .Where(role => role.CompanyId != null && allCompanyIds.Contains(role.CompanyId.Value) && role.Name != null)
            .ToListAsync(HttpContext.RequestAborted);
        var existingRoles = await _userManager.GetRolesAsync(user);
        var roleNamesToRemove = allCompanyRoles
            .Where(role => existingRoles.Contains(role.Name!)
                && (!rolesByCompany.TryGetValue(role.CompanyId!.Value, out var desiredRole) || desiredRole != role.Name))
            .Select(role => role.Name!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var currentMemberships = await _database.UserCompanies
            .Where(membership => membership.UserId == user.Id)
            .ToListAsync(HttpContext.RequestAborted);

        await using var transaction = await _database.Database.BeginTransactionAsync(HttpContext.RequestAborted);
        var removeRolesResult = roleNamesToRemove.Length == 0
            ? IdentityResult.Success
            : await _userManager.RemoveFromRolesAsync(user, roleNamesToRemove);
        if (!removeRolesResult.Succeeded)
        {
            await transaction.RollbackAsync(HttpContext.RequestAborted);
            AddIdentityErrors(removeRolesResult);
            _database.ChangeTracker.Clear();
            await LoadUsersAsync();
            return Page();
        }

        foreach (var membership in submittedMemberships)
        {
            var currentMembership = currentMemberships.SingleOrDefault(item => item.CompanyId == membership.CompanyId);
            if (membership.HasAccess && currentMembership is null)
            {
                _database.UserCompanies.Add(new BlendUserCompany { UserId = user.Id, CompanyId = membership.CompanyId });
            }
            else if (!membership.HasAccess && currentMembership is not null)
            {
                _database.UserCompanies.Remove(currentMembership);
            }
        }

        foreach (var roleName in rolesByCompany.Values.Distinct(StringComparer.Ordinal))
        {
            if (!existingRoles.Contains(roleName, StringComparer.Ordinal) || roleNamesToRemove.Contains(roleName, StringComparer.Ordinal))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync(HttpContext.RequestAborted);
                    AddIdentityErrors(addRoleResult);
                    _database.ChangeTracker.Clear();
                    await LoadUsersAsync();
                    return Page();
                }
            }
        }

        var fallbackCompanyId = selectedMemberships
            .FirstOrDefault(membership => membership.CompanyId == user.CompanyId)?.CompanyId
            ?? selectedMemberships[0].CompanyId;
        var fallbackWorkingCompanyId = selectedMemberships
            .FirstOrDefault(membership => membership.CompanyId == user.WorkingCompanyId)?.CompanyId
            ?? fallbackCompanyId;
        if (user.CompanyId != fallbackCompanyId || user.WorkingCompanyId != fallbackWorkingCompanyId)
        {
            user.CompanyId = fallbackCompanyId;
            user.WorkingCompanyId = fallbackWorkingCompanyId;
            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await transaction.RollbackAsync(HttpContext.RequestAborted);
                AddIdentityErrors(updateUserResult);
                _database.ChangeTracker.Clear();
                await LoadUsersAsync();
                return Page();
            }
        }

        await _database.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        SuccessMessage = $"Company access for '{user.UserName}' was updated. They must sign in again for changes to take effect.";
        ShowMembershipUserId = null;
        await LoadUsersAsync();
        return Page();
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private async Task LoadUsersAsync()
    {
        var isPlatformAdmin = User.IsInRole(BlendRoles.Admin);
        Companies = isPlatformAdmin
            ? await _database.Companies.AsNoTracking().OrderBy(company => company.Name).ToListAsync(HttpContext.RequestAborted)
            : [];

        var claimedCompanyId = Guid.TryParse(User.FindFirst(BlendClaims.CompanyId)?.Value, out var parsedCompanyId) ? parsedCompanyId : (Guid?)null;
        if (!isPlatformAdmin)
        {
            CompanyId = claimedCompanyId?.ToString("D");
        }
        else if (!Guid.TryParse(CompanyId, out var selectedCompanyId) || !await _database.Companies.AnyAsync(company => company.Id == selectedCompanyId, HttpContext.RequestAborted))
        {
            CompanyId = Companies.FirstOrDefault()?.Id.ToString("D");
        }

        CompanyRoles = isPlatformAdmin
            ? await _roleManager.Roles
                .Where(role => role.CompanyId != null && role.Name != null)
                .OrderBy(role => role.Company!.Name)
                .ThenBy(role => role.DisplayName)
                .Select(role => new CompanyRoleOption(role.Id, role.CompanyId!.Value, role.Company!.Name, role.DisplayName))
                .ToListAsync(HttpContext.RequestAborted)
            : [];

        if (!Guid.TryParse(CompanyId, out var companyId))
        {
            Users = [];
            AvailableRoles = [];
            return;
        }

        AvailableRoles = isPlatformAdmin
            ? []
            : await _roleManager.Roles
                .Where(role => role.CompanyId == companyId && role.Name != null)
                .OrderBy(role => role.DisplayName)
                .Select(role => new RoleOption(role.Id, role.DisplayName))
                .ToListAsync(HttpContext.RequestAborted);

        var companyUsers = isPlatformAdmin
            ? await _userManager.Users
                .Where(user => _database.UserCompanies.Any(membership => membership.UserId == user.Id))
                .OrderBy(user => user.UserName)
                .ToListAsync(HttpContext.RequestAborted)
            : await _database.UserCompanies
                .Where(membership => membership.CompanyId == companyId)
                .Select(membership => membership.User)
                .OrderBy(user => user.UserName)
                .ToListAsync(HttpContext.RequestAborted);
        var rolesByName = await _roleManager.Roles
            .Where(role => role.CompanyId == companyId)
            .ToDictionaryAsync(role => role.Name!, role => new RoleAssignment(role.Id, role.DisplayName), StringComparer.Ordinal, HttpContext.RequestAborted);
        var userIds = companyUsers.Select(user => user.Id).ToArray();
        var membershipsByUser = await _database.UserCompanies.AsNoTracking()
            .Where(membership => userIds.Contains(membership.UserId))
            .Select(membership => new { membership.UserId, membership.CompanyId, CompanyName = membership.Company.Name })
            .ToListAsync(HttpContext.RequestAborted);
        var allCompanyRoles = isPlatformAdmin
            ? await _roleManager.Roles.Where(role => role.CompanyId != null && role.Name != null)
                .Select(role => new { role.Id, role.Name, role.DisplayName, role.CompanyId })
                .ToListAsync(HttpContext.RequestAborted)
            : [];

        var rows = new List<UserRow>(companyUsers.Count);
        foreach (var user in companyUsers)
        {
            var assignedRoles = await _userManager.GetRolesAsync(user);
            var userMemberships = membershipsByUser.Where(membership => membership.UserId == user.Id).ToArray();
            var companyRoles = isPlatformAdmin
                ? allCompanyRoles.Where(role => assignedRoles.Contains(role.Name!)
                    && userMemberships.Any(membership => membership.CompanyId == role.CompanyId))
                    .Select(role => new { role.Id, role.DisplayName, role.CompanyId })
                    .ToArray()
                : assignedRoles.Where(rolesByName.ContainsKey)
                    .Select(role => new { rolesByName[role].Id, rolesByName[role].DisplayName, CompanyId = (Guid?)companyId })
                    .ToArray();
            var membershipOptions = isPlatformAdmin
                ? Companies.Select(company =>
                {
                    var membershipExists = userMemberships.Any(membership => membership.CompanyId == company.Id);
                    var companyRole = allCompanyRoles.FirstOrDefault(role => role.CompanyId == company.Id && assignedRoles.Contains(role.Name!));
                    return new CompanyMembershipOption(
                        company.Id,
                        company.Name,
                        membershipExists,
                        companyRole?.Id ?? string.Empty,
                        allCompanyRoles.Where(role => role.CompanyId == company.Id)
                            .OrderBy(role => role.DisplayName)
                            .Select(role => new RoleOption(role.Id, role.DisplayName))
                            .ToArray());
                }).ToArray()
                : [];
            rows.Add(new UserRow(
                user.Id,
                user.UserName ?? string.Empty,
                user.DisplayName,
                user.Email,
                string.Join(", ", companyRoles.Select(role => isPlatformAdmin
                    ? $"{Companies.First(company => company.Id == role.CompanyId).Name}: {role.DisplayName}"
                    : role.DisplayName)),
                companyRoles.FirstOrDefault()?.Id ?? string.Empty,
                user.MustChangePassword,
                userMemberships.Select(membership => membership.CompanyName).OrderBy(name => name).ToArray(),
                membershipOptions));
        }

        Users = rows;
    }

    private async Task<bool> CanManageCompanyAsync(Guid companyId)
    {
        if (User.IsInRole(BlendRoles.Admin))
        {
            return await _database.Companies.AnyAsync(company => company.Id == companyId, HttpContext.RequestAborted);
        }

        return Guid.TryParse(User.FindFirst(BlendClaims.CompanyId)?.Value, out var claimedCompanyId)
            && claimedCompanyId == companyId;
    }

    public sealed class CreateUserInput
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [Required]
        public string RoleId { get; set; } = string.Empty;
    }

    public sealed class EditUserInput
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        public string RoleId { get; set; } = string.Empty;
    }

    public sealed class MembershipFormInput
    {
        public List<CompanyMembershipInput> CompanyMemberships { get; set; } = [];
    }

    public sealed class CompanyMembershipInput
    {
        public Guid CompanyId { get; set; }
        public bool HasAccess { get; set; }
        public string RoleId { get; set; } = string.Empty;
    }

    private sealed record RoleAssignment(string Id, string DisplayName);
    public sealed record RoleOption(string Id, string DisplayName);
    public sealed record CompanyRoleOption(string Id, Guid CompanyId, string CompanyName, string DisplayName);
    public sealed record CompanyMembershipOption(Guid CompanyId, string CompanyName, bool HasAccess, string RoleId, IReadOnlyList<RoleOption> AvailableRoles);
    public sealed record UserRow(string Id, string UserName, string DisplayName, string? Email, string Roles, string RoleId, bool MustChangePassword, IReadOnlyList<string> Companies, IReadOnlyList<CompanyMembershipOption> CompanyMemberships);
}
