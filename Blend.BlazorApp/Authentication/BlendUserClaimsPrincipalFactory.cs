using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BlendIdentityUser, BlendIdentityRole>
{
    public BlendUserClaimsPrincipalFactory(
        UserManager<BlendIdentityUser> userManager,
        RoleManager<BlendIdentityRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BlendIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roleNames = await UserManager.GetRolesAsync(user);
        var roles = await RoleManager.Roles
            .Where(role => roleNames.Contains(role.Name!) && role.CompanyId == user.CompanyId)
            .ToListAsync();

        foreach (var claim in identity.FindAll(ClaimTypes.Role).ToArray())
        {
            identity.RemoveClaim(claim);
        }
        foreach (var claim in identity.FindAll(BlendPermissions.ClaimType).ToArray())
        {
            identity.RemoveClaim(claim);
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role.DisplayName));
            identity.AddClaim(new Claim(BlendClaims.RoleId, role.Id));
            foreach (var claim in await RoleManager.GetClaimsAsync(role))
            {
                identity.AddClaim(claim);
            }
        }

        if (user.CompanyId is { } companyId)
        {
            identity.AddClaim(new Claim(BlendClaims.CompanyId, companyId.ToString("D")));
        }

        identity.AddClaim(new Claim("blend:display-name", user.DisplayName));
        identity.AddClaim(new Claim("blend:must-change-password", user.MustChangePassword.ToString().ToLowerInvariant()));
        return identity;
    }
}
