using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BlendIdentityUser, BlendIdentityRole>
{
    private readonly BlendIdentityDbContext _database;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlendUserClaimsPrincipalFactory(
        UserManager<BlendIdentityUser> userManager,
        RoleManager<BlendIdentityRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
        BlendIdentityDbContext database,
        IHttpContextAccessor httpContextAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _database = database;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BlendIdentityUser user)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var pendingCompanyId = httpContext?.Items[BlendClaims.PendingCompanyId] as Guid?;
        var requestedCompany = pendingCompanyId?.ToString("D")
            ?? httpContext?.User.FindFirst(BlendClaims.CompanyId)?.Value;
        var companyId = Guid.TryParse(requestedCompany, out var selectedCompanyId)
            && await HasMembershipAsync(user.Id, selectedCompanyId)
                ? selectedCompanyId
                : user.CompanyId;
        return await GenerateClaimsAsync(user, companyId);
    }

    public async Task<ClaimsPrincipal> CreatePrincipalAsync(BlendIdentityUser user, Guid companyId)
    {
        if (!await HasMembershipAsync(user.Id, companyId))
        {
            throw new InvalidOperationException("The user does not belong to the selected company.");
        }

        return new ClaimsPrincipal(await GenerateClaimsAsync(user, companyId));
    }

    private async Task<bool> HasMembershipAsync(string userId, Guid companyId) =>
        await _database.UserCompanies.AsNoTracking().AnyAsync(
            membership => membership.UserId == userId && membership.CompanyId == companyId);

    private async Task<ClaimsIdentity> GenerateClaimsAsync(BlendIdentityUser user, Guid? companyId)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roleNames = await UserManager.GetRolesAsync(user);
        var roles = await RoleManager.Roles
            .Where(role => roleNames.Contains(role.Name!)
                && (role.CompanyId == companyId
                    || role.CompanyId == null && role.Name == BlendRoles.Admin))
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

        if (companyId is { } activeCompanyId)
        {
            identity.AddClaim(new Claim(BlendClaims.CompanyId, activeCompanyId.ToString("D")));
        }

        identity.AddClaim(new Claim("blend:display-name", user.DisplayName));
        identity.AddClaim(new Claim("blend:must-change-password", user.MustChangePassword.ToString().ToLowerInvariant()));
        return identity;
    }
}
