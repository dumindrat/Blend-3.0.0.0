using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BlendIdentityUser, IdentityRole>
{
    public BlendUserClaimsPrincipalFactory(
        UserManager<BlendIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BlendIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("blend:display-name", user.DisplayName));
        identity.AddClaim(new Claim("blend:must-change-password", user.MustChangePassword.ToString().ToLowerInvariant()));
        return identity;
    }
}
