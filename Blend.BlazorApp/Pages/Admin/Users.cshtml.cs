using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public IReadOnlyList<RoleOption> AvailableRoles { get; private set; } = [];
    public string? TemporaryPassword { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task OnGetAsync() => await LoadUsersAsync();

    public async Task<IActionResult> OnPostAsync()
    {
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
        var roleNames = await _roleManager.Roles
            .Where(role => role.CompanyId == companyId)
            .ToDictionaryAsync(role => role.Name!, role => role.DisplayName, StringComparer.Ordinal, HttpContext.RequestAborted);
        var rows = new List<UserRow>(companyUsers.Count);
        foreach (var user in companyUsers)
        {
            var assignedRoles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRow(user.UserName ?? string.Empty, user.DisplayName, string.Join(", ", assignedRoles.Where(roleNames.ContainsKey).Select(role => roleNames[role])), user.MustChangePassword));
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

    public sealed record RoleOption(string Id, string DisplayName);
    public sealed record UserRow(string UserName, string DisplayName, string Roles, bool MustChangePassword);
}
