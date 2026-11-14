using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blend.BlazorApp.Pages.Account;

[AllowAnonymous]
public sealed class CompanyAccessUnavailableModel : PageModel
{
    public void OnGet()
    {
    }
}
