using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Layout;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class MainLayoutTests
{
    [TestMethod]
    public void AuthenticatedUserAppearsInTopbar()
    {
        using var context = new Bunit.TestContext();
        context.Services.AddSingleton<IDbContextFactory<BlendIdentityDbContext>>(new TestDatabaseFactory());
        var authorization = context.AddTestAuthorization();
        authorization.SetAuthorized("test-user");

        var layout = context.RenderComponent<MainLayout>();

        Assert.AreEqual("Signed in astest-user", layout.Find(".topbar-user").TextContent.Trim());
    }

    [TestMethod]
    public void AnonymousUserDoesNotAppearInTopbar()
    {
        using var context = new Bunit.TestContext();
        context.Services.AddSingleton<IDbContextFactory<BlendIdentityDbContext>>(new TestDatabaseFactory());
        context.AddTestAuthorization().SetNotAuthorized();

        var layout = context.RenderComponent<MainLayout>();

        Assert.AreEqual(0, layout.FindAll(".topbar-user").Count);
    }
}
