using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Roles = BlendRoles.Admin, Policy = "PasswordChangeComplete")]
public sealed class RolesModel : PageModel
{
    private static readonly HashSet<string> SystemRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        BlendRoles.Admin,
        BlendRoles.User
    };

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BlendIdentityDbContext _database;

    public RolesModel(RoleManager<IdentityRole> roleManager, BlendIdentityDbContext database)
    {
        _roleManager = roleManager;
        _database = database;
    }

    [BindProperty]
    public RoleInput Input { get; set; } = new();

    [BindProperty]
    public MenuAssignmentInput MenuAssignment { get; set; } = new();

    public IReadOnlyList<RoleRow> Roles { get; private set; } = [];
    public IReadOnlyList<MenuItemOption> MenuItems { get; private set; } = [];
    public IReadOnlyDictionary<string, IReadOnlySet<int>> AssignedMenuItemIds { get; private set; } = new Dictionary<string, IReadOnlySet<int>>();
    public string? SuccessMessage { get; private set; }

    public async Task OnGetAsync() => await LoadRolesAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        if (ModelState.IsValid && !SystemRoles.Contains(Input.Name))
        {
            var role = new IdentityRole(Input.Name);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                var permissionResult = await SetManageUsersPermissionAsync(role, Input.CanManageUsers);
                if (permissionResult.Count == 0)
                {
                    SuccessMessage = $"Role '{role.Name}' created.";
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
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(Input.Id);
        if (role is null)
        {
            ModelState.AddModelError(string.Empty, "The selected role no longer exists.");
            await LoadRolesAsync();
            return Page();
        }

        if (SystemRoles.Contains(role.Name ?? string.Empty))
        {
            ModelState.AddModelError(string.Empty, "The built-in Admin and User roles cannot be changed.");
            await LoadRolesAsync();
            return Page();
        }

        if (SystemRoles.Contains(Input.Name))
        {
            ModelState.AddModelError(nameof(Input.Name), "The built-in Admin and User role names are reserved.");
            await LoadRolesAsync();
            return Page();
        }

        role.Name = Input.Name;
        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            AddErrors(updateResult);
            await LoadRolesAsync();
            return Page();
        }

        var permissionErrors = await SetManageUsersPermissionAsync(role, Input.CanManageUsers);
        if (permissionErrors.Count > 0)
        {
            foreach (var error in permissionErrors)
            {
                ModelState.AddModelError(string.Empty, $"Role name was updated, but its permission could not be saved: {error}");
            }
        }
        else
        {
            SuccessMessage = $"Role '{role.Name}' updated. Users assigned this role must sign out and back in for permission changes to take effect.";
        }

        await LoadRolesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignMenuItemsAsync()
    {
        var role = await _roleManager.FindByIdAsync(MenuAssignment.RoleId);
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
            .Where(item => selectedIds.Contains(item.Id))
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
        SuccessMessage = $"Menu item assignments for role '{role.Name}' saved.";
        await LoadRolesAsync();
        return Page();
    }

    private async Task LoadRolesAsync()
    {
        MenuItems = await _database.MenuItems
            .AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => new MenuItemOption(item.Id, item.Name, item.GroupName))
            .ToListAsync(HttpContext.RequestAborted);
        var assignments = await _database.MenuItemRoles
            .AsNoTracking()
            .GroupBy(assignment => assignment.RoleId)
            .Select(group => new { RoleId = group.Key, MenuItemIds = group.Select(assignment => assignment.MenuItemId).ToList() })
            .ToListAsync(HttpContext.RequestAborted);
        AssignedMenuItemIds = assignments.ToDictionary(
            assignment => assignment.RoleId,
            assignment => (IReadOnlySet<int>)assignment.MenuItemIds.ToHashSet(),
            StringComparer.Ordinal);

        var rows = new List<RoleRow>();
        foreach (var role in _roleManager.Roles.OrderBy(role => role.Name))
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            rows.Add(new RoleRow(
                role.Id,
                role.Name ?? string.Empty,
                claims.Any(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == BlendPermissions.ManageUsers),
                SystemRoles.Contains(role.Name ?? string.Empty)));
        }

        Roles = rows;
    }

    private async Task<IReadOnlyList<string>> SetManageUsersPermissionAsync(IdentityRole role, bool enabled)
    {
        var errors = new List<string>();
        var permissionClaims = (await _roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == BlendPermissions.ClaimType && claim.Value == BlendPermissions.ManageUsers)
            .ToArray();

        if (enabled && permissionClaims.Length == 0)
        {
            var result = await _roleManager.AddClaimAsync(role, new Claim(BlendPermissions.ClaimType, BlendPermissions.ManageUsers));
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(error => error.Description));
            }
        }
        else if (!enabled)
        {
            foreach (var claim in permissionClaims)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors.Select(error => error.Description));
                }
            }
        }

        return errors;
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

    public sealed record MenuItemOption(int Id, string Name, string GroupName);

    public sealed class RoleInput
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        public bool CanManageUsers { get; set; }
    }

    public sealed record RoleRow(string Id, string Name, bool CanManageUsers, bool IsSystemRole);
}
