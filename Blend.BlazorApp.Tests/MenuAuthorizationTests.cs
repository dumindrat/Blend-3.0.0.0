using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class MenuAuthorizationTests
{
    [TestMethod]
    public async Task AssignedRoleAllowsMatchingPath()
    {
        var result = await InvokeAsync(assigned: true, requestPath: "/brokers/details");
        Assert.IsTrue(result.NextCalled);
        Assert.IsNull(result.Authentication.Action);
    }

    [TestMethod]
    public async Task UnassignedRoleIsForbidden()
    {
        var result = await InvokeAsync(assigned: false, requestPath: "/Brokers");
        Assert.IsFalse(result.NextCalled);
        Assert.AreEqual("forbid", result.Authentication.Action);
    }

    [TestMethod]
    public async Task AnonymousUserIsChallengedForMenuPath()
    {
        var result = await InvokeAsync(assigned: false, requestPath: "/Brokers", anonymous: true);
        Assert.IsFalse(result.NextCalled);
        Assert.AreEqual("challenge", result.Authentication.Action);
    }

    [TestMethod]
    public async Task SimilarPathDoesNotMatchMenuSegment()
    {
        var result = await InvokeAsync(assigned: false, requestPath: "/BrokersExtra");
        Assert.IsTrue(result.NextCalled);
        Assert.IsNull(result.Authentication.Action);
    }

    private static async Task<(bool NextCalled, RecordingAuthentication Authentication)> InvokeAsync(bool assigned, string requestPath, bool anonymous = false)
    {
        var factory = new TestDatabaseFactory();
        var company = TestData.Company();
        var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
        await using (var db = factory.CreateDbContext())
        {
            db.Companies.Add(company);
            db.Roles.Add(role);
            db.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Brokers", Path = "/Brokers" });
            if (assigned) db.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            await db.SaveChangesAsync();
        }
        var authentication = new RecordingAuthentication();
        using var services = new ServiceCollection().AddSingleton<IAuthenticationService>(authentication).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        if (!anonymous) context.User = TestData.Principal(company.Id, roleId: role.Id);
        context.Request.Path = requestPath;
        var nextCalled = false;
        await new BlendMenuItemAuthorizationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(context, factory);
        return (nextCalled, authentication);
    }

    private sealed class RecordingAuthentication : IAuthenticationService
    {
        public string? Action { get; private set; }
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) { Action = "challenge"; return Task.CompletedTask; }
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) { Action = "forbid"; return Task.CompletedTask; }
        public Task SignInAsync(HttpContext context, string? scheme, System.Security.Claims.ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }
}
