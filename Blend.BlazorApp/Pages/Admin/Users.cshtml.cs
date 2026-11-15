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

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public IReadOnlyList<RoleOption> AvailableRoles { get; private set; } = [];
    public string? TemporaryPassword { get; private set; }
    public string? SuccessMessage { get; private set; }
    public bool ShowCreateDialog { get; private set; }
    public string? ShowEditUserId { get; private set; }

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
                SuccessMessage = "User created. Copy the temporary password now; it will not be shown again.";
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

        if (!Guid.TryParse(CompanyId, out var companyId) || !await CanManageCompanyAsync(companyId))
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
            await LoadUsersAsync();
            return Page();
        }

        var user = await _userManager.Users.SingleOrDefaultAsync(
            candidate => candidate.Id == id && candidate.CompanyId == companyId,
            HttpContext.RequestAborted);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The selected user does not belong to this company or no longer exists.");
            await LoadUsersAsync();
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(EditInput.RoleId);
        if (role is null || role.CompanyId != companyId || string.IsNullOrWhiteSpace(role.Name))
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

        var existingRoles = await _userManager.GetRolesAsync(user);
        var removeResult = existingRoles.Count == 0
            ? IdentityResult.Success
            : await _userManager.RemoveFromRolesAsync(user, existingRoles);
        if (!removeResult.Succeeded)
        {
            await transaction.RollbackAsync(HttpContext.RequestAborted);
            AddIdentityErrors(removeResult);
            _database.ChangeTracker.Clear();
            await LoadUsersAsync();
            return Page();
        }

        var addResult = await _userManager.AddToRoleAsync(user, role.Name);
        if (!addResult.Succeeded)
        {
            await transaction.RollbackAsync(HttpContext.RequestAborted);
            AddIdentityErrors(addResult);
            _database.ChangeTracker.Clear();
            await LoadUsersAsync();
            return Page();
        }

        await transaction.CommitAsync(HttpContext.RequestAborted);
        SuccessMessage = $"User '{user.UserName}' updated. They must sign in again for role changes to take effect.";
        ShowEditUserId = null;
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

        if (!Guid.TryParse(CompanyId, out var companyId))
        {
            Users = [];
            AvailableRoles = [];
            return;
        }

        AvailableRoles = await _roleManager.Roles
            .Where(role => role.CompanyId == companyId && role.Name != null)
            .OrderBy(role => role.DisplayName)
            .Select(role => new RoleOption(role.Id, role.DisplayName))
            .ToListAsync(HttpContext.RequestAborted);

        var companyUsers = await _userManager.Users
            .Where(user => user.CompanyId == companyId)
            .OrderBy(user => user.UserName)
            .ToListAsync(HttpContext.RequestAborted);
        var rolesByName = await _roleManager.Roles
            .Where(role => role.CompanyId == companyId)
            .ToDictionaryAsync(role => role.Name!, role => new RoleAssignment(role.Id, role.DisplayName), StringComparer.Ordinal, HttpContext.RequestAborted);
        var rows = new List<UserRow>(companyUsers.Count);
        foreach (var user in companyUsers)
        {
            var assignedRoles = await _userManager.GetRolesAsync(user);
            var companyRoles = assignedRoles.Where(rolesByName.ContainsKey).Select(role => rolesByName[role]).ToArray();
            rows.Add(new UserRow(
                user.Id,
                user.UserName ?? string.Empty,
                user.DisplayName,
                user.Email,
                string.Join(", ", companyRoles.Select(role => role.DisplayName)),
                companyRoles.FirstOrDefault()?.Id ?? string.Empty,
                user.MustChangePassword));
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

        [Required]
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

        [Required]
        public string RoleId { get; set; } = string.Empty;
    }

    private sealed record RoleAssignment(string Id, string DisplayName);
    public sealed record RoleOption(string Id, string DisplayName);
    public sealed record UserRow(string Id, string UserName, string DisplayName, string? Email, string Roles, string RoleId, bool MustChangePassword);
}
