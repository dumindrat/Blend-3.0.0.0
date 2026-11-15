using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Account;

[Authorize]
public sealed class SwitchCompanyModel : PageModel
{
    private readonly BlendIdentityDbContext _database;
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly BlendUserClaimsPrincipalFactory _claimsPrincipalFactory;

    public SwitchCompanyModel(
        BlendIdentityDbContext database,
        UserManager<BlendIdentityUser> userManager,
        BlendUserClaimsPrincipalFactory claimsPrincipalFactory)
    {
        _database = database;
        _userManager = userManager;
        _claimsPrincipalFactory = claimsPrincipalFactory;
    }

    public async Task<IActionResult> OnPostAsync(Guid companyId, string? returnUrl)
    {
        if (User.IsInRole(BlendRoles.Admin))
        {
            return BadRequest();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasAccess = await _database.UserCompanies
            .Where(membership => membership.UserId == user.Id && membership.CompanyId == companyId)
            .Select(membership => membership.Company)
            .AnyAsync(company => company.IsEnabled
                && today >= company.SubscriptionStartDate
                && today <= company.SubscriptionEndDate,
                HttpContext.RequestAborted);
        if (!hasAccess)
        {
            return Forbid();
        }

        var principal = await _claimsPrincipalFactory.CreatePrincipalAsync(user, companyId);
        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        return LocalRedirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
