using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blend.BlazorApp.Pages.Account;

[Authorize]
public sealed class LogoutModel : PageModel
{
    private readonly SignInManager<BlendIdentityUser> _signInManager;

    public LogoutModel(SignInManager<BlendIdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult OnGet() => LocalRedirect("/");

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/Account/Login");
    }
}
