using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeCompany = await _database.UserCompanies
                .Where(membership => membership.UserId == user.Id
                    && membership.Company.IsEnabled
                    && today >= membership.Company.SubscriptionStartDate
                    && today <= membership.Company.SubscriptionEndDate)
                .OrderBy(membership => membership.CompanyId == user.CompanyId ? 0 : 1)
                .ThenBy(membership => membership.Company.Name)
                .Select(membership => membership.Company)
                .FirstOrDefaultAsync(HttpContext.RequestAborted);
            if (activeCompany is null)
            {
                ModelState.AddModelError(string.Empty, "Company access is unavailable. Contact your administrator.");
                return Page();
            }

            HttpContext.Items[BlendClaims.PendingCompanyId] = activeCompany.Id;
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
