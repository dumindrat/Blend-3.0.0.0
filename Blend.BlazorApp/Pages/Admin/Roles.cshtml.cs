using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Policy = "PasswordChangeComplete")]
[Authorize(Policy = BlendPermissions.ManageRolesPolicy)]
public sealed class RolesModel : PageModel
{
    private static readonly HashSet<string> SystemRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        BlendRoles.Admin,
        BlendRoles.User
    };

    private readonly RoleManager<BlendIdentityRole> _roleManager;
    private readonly BlendIdentityDbContext _database;

    public RolesModel(RoleManager<BlendIdentityRole> roleManager, BlendIdentityDbContext database)
    {
        _roleManager = roleManager;
        _database = database;
    }

    [BindProperty]
    public RoleInput Input { get; set; } = new();

    [BindProperty]
    public MenuAssignmentInput MenuAssignment { get; set; } = new();

    [BindProperty]
    public OperationAssignmentInput OperationAssignment { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<RoleRow> Roles { get; private set; } = [];
    public IReadOnlyList<MenuItemOption> MenuItems { get; private set; } = [];
    public IReadOnlyList<OperationOption> Operations { get; private set; } = [];
    public IReadOnlyDictionary<string, IReadOnlySet<int>> AssignedMenuItemIds { get; private set; } = new Dictionary<string, IReadOnlySet<int>>();
    public IReadOnlyDictionary<string, IReadOnlySet<int>> AssignedOperationIds { get; private set; } = new Dictionary<string, IReadOnlySet<int>>();
    public string? SuccessMessage { get; private set; }
    public bool ShowCreateDialog { get; private set; }
    public string? ShowEditRoleId { get; private set; }
    public string? ShowOperationRoleId { get; private set; }

    public async Task OnGetAsync() => await LoadRolesAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ShowCreateDialog = true;
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        var companyId = Guid.TryParse(Input.CompanyId, out var selectedCompanyId) ? selectedCompanyId : (Guid?)null;
        CompanyId = companyId?.ToString("D");
        if (companyId is null || !await CanManageCompanyAsync(companyId.Value))
        {
            ModelState.AddModelError(nameof(Input.CompanyId), "Select a company you are allowed to manage.");
        }

        if (ModelState.IsValid && !SystemRoles.Contains(Input.Name))
        {
            if (await _roleManager.Roles.AnyAsync(role => role.CompanyId == companyId && role.DisplayName == Input.Name, HttpContext.RequestAborted))
            {
                ModelState.AddModelError(nameof(Input.Name), "A role with this name already exists in the company.");
                await LoadRolesAsync();
                return Page();
            }

            var role = new BlendIdentityRole($"{companyId:N}:{Guid.NewGuid():N}", Input.Name, companyId);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                var permissionResult = await SetRolePermissionsAsync(role, Input.CanManageUsers, Input.CanManageRoles, Input.CanManageMenuItems);
                if (permissionResult.Count == 0)
                {
                    SuccessMessage = $"Role '{role.DisplayName}' created.";
                    ShowCreateDialog = false;
                    Input = new RoleInput();
                }
                else
                {
                    foreach (var error in permissionResult)
                    {
                        ModelState.AddModelError(string.Empty, $"Role was created, but its permission could not be saved: {error}");
                    }
                }
            }
            else
            {
                AddErrors(result);
            }
        }
        else if (SystemRoles.Contains(Input.Name))
        {
            ModelState.AddModelError(nameof(Input.Name), "The built-in Admin and User roles are reserved.");
        }

        await LoadRolesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        ShowEditRoleId = Input.Id;
        CompanyId = Input.CompanyId;
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(Input.Id);
        if (role is not null && role.CompanyId is null)
        {
            ModelState.AddModelError(string.Empty, "Platform roles cannot be changed from company role management.");
            await LoadRolesAsync();
            return Page();
        }

        if (role is null)
        {
            ModelState.AddModelError(string.Empty, "The selected role no longer exists.");
            await LoadRolesAsync();
            return Page();
        }

        if (role.CompanyId is null || SystemRoles.Contains(role.DisplayName))
        {
            ModelState.AddModelError(string.Empty, "Built-in roles cannot be changed from company role management.");
            await LoadRolesAsync();
            return Page();
        }

        if (SystemRoles.Contains(Input.Name))
        {
            ModelState.AddModelError(nameof(Input.Name), "The built-in Admin and User role names are reserved.");
            await LoadRolesAsync();
            return Page();
        }

        if (!Guid.TryParse(Input.CompanyId, out var updateCompanyId)
            || role.CompanyId != updateCompanyId
            || !await CanManageCompanyAsync(updateCompanyId))
        {
            ModelState.AddModelError(string.Empty, "The role does not belong to a company you are allowed to manage.");
            await LoadRolesAsync();
            return Page();
        }

        if (await _roleManager.Roles.AnyAsync(
            candidate => candidate.CompanyId == updateCompanyId && candidate.Id != role.Id && candidate.DisplayName == Input.Name,
            HttpContext.RequestAborted))
        {
            ModelState.AddModelError(nameof(Input.Name), "A role with this name already exists in the company.");
            await LoadRolesAsync();
            return Page();
        }

        role.DisplayName = Input.Name;
        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            AddErrors(updateResult);
            await LoadRolesAsync();
            return Page();
        }

        var permissionErrors = await SetRolePermissionsAsync(role, Input.CanManageUsers, Input.CanManageRoles, Input.CanManageMenuItems);
        if (permissionErrors.Count > 0)
        {
            foreach (var error in permissionErrors)
            {
                ModelState.AddModelError(string.Empty, $"Role name was updated, but its permission could not be saved: {error}");
            }
        }
        else
        {
            SuccessMessage = $"Role '{role.DisplayName}' updated. Users assigned this role must sign out and back in for permission changes to take effect.";
            ShowEditRoleId = null;
        }

        await LoadRolesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignOperationsAsync()
    {
        ModelState.Clear();
        var role = await _roleManager.FindByIdAsync(OperationAssignment.RoleId);
        if (role is null)
        {
            ModelState.AddModelError(string.Empty, "The selected role no longer exists.");
            await LoadRolesAsync();
            return Page();
        }

        if (!Guid.TryParse(CompanyId, out var companyId)
            || role.CompanyId != companyId
            || !await CanManageCompanyAsync(companyId))
        {
            ModelState.AddModelError(string.Empty, "The role does not belong to a company you are allowed to manage.");
            await LoadRolesAsync();
            return Page();
        }

        ShowOperationRoleId = role.Id;
        var selectedIds = (OperationAssignment.OperationIds ?? []).Distinct().ToArray();
        var validIds = await _database.MenuItemOperations
            .Where(operation => selectedIds.Contains(operation.Id) && operation.MenuItem.CompanyId == companyId)
            .Select(operation => operation.Id)
            .ToListAsync(HttpContext.RequestAborted);
        if (validIds.Count != selectedIds.Length)
        {
            ModelState.AddModelError(string.Empty, "One or more selected operations are not part of this company.");
            await LoadRolesAsync();
            return Page();
        }

        var assignments = await _database.RoleOperations
            .Where(assignment => assignment.RoleId == role.Id)
            .ToListAsync(HttpContext.RequestAborted);
        var selectedSet = selectedIds.ToHashSet();
        _database.RoleOperations.RemoveRange(assignments.Where(assignment => !selectedSet.Contains(assignment.OperationId)));
        var existingIds = assignments.Select(assignment => assignment.OperationId).ToHashSet();
        foreach (var operationId in selectedIds.Where(operationId => !existingIds.Contains(operationId)))
        {
            _database.RoleOperations.Add(new BlendRoleOperation { RoleId = role.Id, OperationId = operationId });
        }

        await _database.SaveChangesAsync(HttpContext.RequestAborted);
        SuccessMessage = $"Operation permissions for role '{role.DisplayName}' saved.";
        ShowOperationRoleId = null;
        await LoadRolesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignMenuItemsAsync()
    {
        var role = await _roleManager.FindByIdAsync(MenuAssignment.RoleId);
        if (role is not null && (role.CompanyId is null || !Guid.TryParse(CompanyId, out var assignmentCompanyId)
            || role.CompanyId != assignmentCompanyId
            || !await CanManageCompanyAsync(assignmentCompanyId)))
        {
            ModelState.AddModelError(string.Empty, "The role does not belong to a company you are allowed to manage.");
            await LoadRolesAsync();
            return Page();
        }

        if (role is null)
        {
            ModelState.AddModelError(string.Empty, "The selected role no longer exists.");
            await LoadRolesAsync();
            return Page();
        }

        if (string.Equals(role.Name, BlendRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "The built-in Admin role already has access to all menu items.");
            await LoadRolesAsync();
            return Page();
        }

        var selectedIds = (MenuAssignment.MenuItemIds ?? []).Distinct().ToArray();
        var validIds = await _database.MenuItems
            .Where(item => selectedIds.Contains(item.Id) && item.CompanyId == role.CompanyId)
            .Select(item => item.Id)
            .ToListAsync(HttpContext.RequestAborted);
        if (validIds.Count != selectedIds.Length)
        {
            ModelState.AddModelError(string.Empty, "One or more selected menu items no longer exist.");
            await LoadRolesAsync();
            return Page();
        }

        var assignments = await _database.MenuItemRoles
            .Where(assignment => assignment.RoleId == role.Id)
            .ToListAsync(HttpContext.RequestAborted);
        var selectedSet = selectedIds.ToHashSet();
        _database.MenuItemRoles.RemoveRange(assignments.Where(assignment => !selectedSet.Contains(assignment.MenuItemId)));
        var existingIds = assignments.Select(assignment => assignment.MenuItemId).ToHashSet();
        foreach (var menuItemId in selectedIds.Where(menuItemId => !existingIds.Contains(menuItemId)))
        {
            _database.MenuItemRoles.Add(new BlendMenuItemRole { RoleId = role.Id, MenuItemId = menuItemId });
        }

        await _database.SaveChangesAsync(HttpContext.RequestAborted);
        SuccessMessage = $"Menu item assignments for role '{role.DisplayName}' saved.";
        await LoadRolesAsync();
        return Page();
    }

    private async Task LoadRolesAsync()
    {
        if (User.IsInRole(BlendRoles.Admin))
        {
            Companies = await _database.Companies.AsNoTracking().OrderBy(company => company.Name).ToListAsync(HttpContext.RequestAborted);
        }
        else if (Guid.TryParse(User.FindFirst(BlendClaims.CompanyId)?.Value, out var claimedCompanyId))
        {
            Companies = await _database.Companies.AsNoTracking().Where(company => company.Id == claimedCompanyId).ToListAsync(HttpContext.RequestAborted);
            CompanyId = claimedCompanyId.ToString("D");
        }
        else
        {
            Companies = [];
        }

        var selectedCompanyId = Guid.TryParse(CompanyId, out var parsedCompanyId) && Companies.Any(company => company.Id == parsedCompanyId)
            ? parsedCompanyId
            : Companies.FirstOrDefault()?.Id;
        CompanyId = selectedCompanyId?.ToString("D");
        MenuItems = selectedCompanyId is null ? [] : await _database.MenuItems
            .AsNoTracking()
            .Where(item => item.CompanyId == selectedCompanyId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new MenuItemOption(item.Id, item.Name, item.GroupName))
            .ToListAsync(HttpContext.RequestAborted);
        Operations = selectedCompanyId is null ? [] : await _database.MenuItemOperations
            .AsNoTracking()
            .Where(operation => operation.MenuItem.CompanyId == selectedCompanyId)
            .OrderBy(operation => operation.MenuItem.SortOrder)
            .ThenBy(operation => operation.MenuItem.Name)
            .ThenBy(operation => operation.SortOrder)
            .Select(operation => new OperationOption(operation.Id, operation.MenuItemId, operation.MenuItem.Name, operation.Name))
            .ToListAsync(HttpContext.RequestAborted);
        var assignments = await _database.MenuItemRoles
            .AsNoTracking()
            .Where(assignment => selectedCompanyId != null && assignment.Role.CompanyId == selectedCompanyId)
            .GroupBy(assignment => assignment.RoleId)
            .Select(group => new { RoleId = group.Key, MenuItemIds = group.Select(assignment => assignment.MenuItemId).ToList() })
            .ToListAsync(HttpContext.RequestAborted);
        AssignedMenuItemIds = assignments.ToDictionary(
            assignment => assignment.RoleId,
            assignment => (IReadOnlySet<int>)assignment.MenuItemIds.ToHashSet(),
            StringComparer.Ordinal);
        var operationAssignments = await _database.RoleOperations
            .AsNoTracking()
            .Where(assignment => selectedCompanyId != null && assignment.Role.CompanyId == selectedCompanyId)
            .GroupBy(assignment => assignment.RoleId)
            .Select(group => new { RoleId = group.Key, OperationIds = group.Select(assignment => assignment.OperationId).ToList() })
            .ToListAsync(HttpContext.RequestAborted);
        AssignedOperationIds = operationAssignments.ToDictionary(
            assignment => assignment.RoleId,
            assignment => (IReadOnlySet<int>)assignment.OperationIds.ToHashSet(),
            StringComparer.Ordinal);

        var rows = new List<RoleRow>();
        if (selectedCompanyId is not null)
        {
            foreach (var role in _roleManager.Roles.Where(role => role.CompanyId == selectedCompanyId).OrderBy(role => role.DisplayName))
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                rows.Add(new RoleRow(
                    role.Id,
                    role.DisplayName,
                    claims.Any(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == BlendPermissions.ManageUsers),
                    claims.Any(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == BlendPermissions.ManageRoles),
                    claims.Any(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == BlendPermissions.ManageMenuItems),
                    role.DisplayName.Equals(BlendRoles.User, StringComparison.OrdinalIgnoreCase)));
            }
        }

        Roles = rows;
    }

    private async Task<IReadOnlyList<string>> SetRolePermissionsAsync(BlendIdentityRole role, bool canManageUsers, bool canManageRoles, bool canManageMenuItems)
    {
        var errors = new List<string>();
        await SetPermissionAsync(role, BlendPermissions.ManageUsers, canManageUsers, errors);
        await SetPermissionAsync(role, BlendPermissions.ManageRoles, canManageRoles, errors);
        await SetPermissionAsync(role, BlendPermissions.ManageMenuItems, canManageMenuItems, errors);
        return errors;
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

    private async Task SetPermissionAsync(BlendIdentityRole role, string permission, bool enabled, List<string> errors)
    {
        var existingClaims = (await _roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == permission)
            .ToArray();
        if (enabled && existingClaims.Length == 0)
        {
            var result = await _roleManager.AddClaimAsync(role, new Claim(BlendPermissions.ClaimType, permission));
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(error => error.Description));
            }
        }
        else if (!enabled)
        {
            foreach (var claim in existingClaims)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors.Select(error => error.Description));
                }
            }
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public sealed class MenuAssignmentInput
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> MenuItemIds { get; set; } = [];
    }

    public sealed class OperationAssignmentInput
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> OperationIds { get; set; } = [];
    }

    public sealed record MenuItemOption(int Id, string Name, string GroupName);
    public sealed record OperationOption(int Id, int MenuItemId, string MenuItemName, string Name);

    public sealed class RoleInput
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? CompanyId { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanManageRoles { get; set; }
        public bool CanManageMenuItems { get; set; }
    }

    public sealed record RoleRow(string Id, string Name, bool CanManageUsers, bool CanManageRoles, bool CanManageMenuItems, bool IsSystemRole);
}
