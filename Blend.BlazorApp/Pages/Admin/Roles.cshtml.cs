using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    public RolesModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty]
    public RoleInput Input { get; set; } = new();

    public IReadOnlyList<RoleRow> Roles { get; private set; } = [];
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

    private async Task LoadRolesAsync()
    {
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
