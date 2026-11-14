using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Roles = BlendRoles.Admin, Policy = "PasswordChangeComplete")]
public sealed class UsersModel : PageModel
{
    private readonly IUserAdministrationService _userAdministration;
    private readonly UserManager<BlendIdentityUser> _userManager;

    public UsersModel(IUserAdministrationService userAdministration, UserManager<BlendIdentityUser> userManager)
    {
        _userAdministration = userAdministration;
        _userManager = userManager;
    }

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public string? TemporaryPassword { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task OnGetAsync() => await LoadUsersAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result = await _userAdministration.CreateUserAsync(
                Input.UserName,
                Input.DisplayName,
                Input.Email,
                Input.Role,
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
        var users = await _userManager.Users
            .OrderBy(user => user.UserName)
            .ToListAsync(HttpContext.RequestAborted);
        var rows = new List<UserRow>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRow(user.UserName ?? string.Empty, user.DisplayName, string.Join(", ", roles), user.MustChangePassword));
        }

        Users = rows;
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
        public string Role { get; set; } = BlendRoles.User;
    }

    public sealed record UserRow(string UserName, string DisplayName, string Roles, bool MustChangePassword);
}
