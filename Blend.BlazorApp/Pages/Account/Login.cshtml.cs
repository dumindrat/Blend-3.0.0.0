using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blend.BlazorApp.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly SignInManager<BlendIdentityUser> _signInManager;
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly BlendIdentityDbContext _database;

    public LoginModel(
        SignInManager<BlendIdentityUser> signInManager,
        UserManager<BlendIdentityUser> userManager,
        BlendIdentityDbContext database)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _database = database;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByNameAsync(Input.UserName.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                passwordResult.IsLockedOut ? "This account is temporarily locked. Try again later." : "Invalid username or password.");
            return Page();
        }

        var isPlatformAdmin = await _userManager.IsInRoleAsync(user, BlendRoles.Admin);
        if (!isPlatformAdmin)
        {
            var company = user.CompanyId is { } companyId
                ? await _database.Companies.FindAsync([companyId], HttpContext.RequestAborted)
                : null;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (company is null || !company.IsEnabled || today < company.SubscriptionStartDate || today > company.SubscriptionEndDate)
            {
                ModelState.AddModelError(string.Empty, "Company access is unavailable. Contact your administrator.");
                return Page();
            }
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            isPersistent: false,
            lockoutOnFailure: true);
        if (signInResult.Succeeded)
        {
            return LocalRedirect(user.MustChangePassword ? "/Account/ChangePassword" : GetSafeReturnUrl());
        }

        ModelState.AddModelError(string.Empty,
            signInResult.IsLockedOut ? "This account is temporarily locked. Try again later." : "Invalid username or password.");
        return Page();
    }

    private string GetSafeReturnUrl() =>
        !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/";

    public sealed class LoginInput
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
