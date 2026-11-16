using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Tests;

internal sealed class TestDatabaseFactory : IDbContextFactory<BlendIdentityDbContext>
{
    private readonly DbContextOptions<BlendIdentityDbContext> _options =
        new DbContextOptionsBuilder<BlendIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    public BlendIdentityDbContext CreateDbContext() => new(_options);
}

internal static class TestData
{
    public static BlendCompany Company(int number = 1) => new()
    {
        Id = Guid.NewGuid(), CompanyNumber = number, Name = "Test Company",
        SubscriptionStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
        SubscriptionEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), IsEnabled = true
    };

    public static ClaimsPrincipal Principal(Guid? companyId, bool admin = false, string? roleId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Name, "test-user")
        };
        if (companyId is not null) claims.Add(new Claim(BlendClaims.CompanyId, companyId.ToString()!));
        if (admin) claims.Add(new Claim(ClaimTypes.Role, BlendRoles.Admin));
        if (roleId is not null) claims.Add(new Claim(BlendClaims.RoleId, roleId));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
