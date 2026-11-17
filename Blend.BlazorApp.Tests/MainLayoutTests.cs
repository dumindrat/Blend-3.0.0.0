using System.Security.Claims;
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

    [TestMethod]
    public void UserSeesOnlyAssignedMenuItemsInWorkingCompany()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1");
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.AreEqual(1, layout.FindAll("a[href='/Brokers']").Count);
        Assert.AreEqual(0, layout.FindAll("a[href='/Buyers']").Count);
        Assert.AreEqual(0, layout.FindAll("a[href='/OtherCompany']").Count);
    }

    [TestMethod]
    public void UserWithoutAssignedMenuSeesNoBusinessMenuItems()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("unassigned-role");
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.AreEqual(0, layout.FindAll(".module-nav-group a").Count);
        Assert.AreEqual(0, layout.FindAll("a[href='/Brokers'], a[href='/Buyers']").Count);
    }

    [TestMethod]
    public void MultipleRolesShowUnionOfAssignedMenuItems()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1", "role-2");
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.AreEqual(1, layout.FindAll("a[href='/Brokers']").Count);
        Assert.AreEqual(1, layout.FindAll("a[href='/Buyers']").Count);
    }

    [TestMethod]
    public void UserWithoutWorkingCompanySeesNoBusinessMenuItems()
    {
        using var context = new Bunit.TestContext();
        context.Services.AddSingleton<IDbContextFactory<BlendIdentityDbContext>>(new TestDatabaseFactory());
        var authorization = context.AddTestAuthorization();
        authorization.SetAuthorized("test-user");
        authorization.SetClaims(new Claim(BlendClaims.RoleId, "role-1"));
        var layout = context.RenderComponent<MainLayout>();

        Assert.AreEqual(0, layout.FindAll(".module-nav-group a").Count);
        Assert.AreEqual(0, layout.FindAll("a[href='/Brokers'], a[href='/Buyers']").Count);
    }

    [TestMethod]
    public void ActiveAssignedMenuIsHighlighted()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1");
        fixture.Context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/Brokers");
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.IsTrue(layout.Find("a[href='/Brokers']").ClassList.Contains("active"));
        Assert.IsFalse(layout.Find("a[href='/']").ClassList.Contains("active"));
    }

    [TestMethod]
    public void PlatformAdminSeesAllWorkingCompanyMenuItemsWithoutAssignments()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize(admin: true);
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.AreEqual(1, layout.FindAll("a[href='/Brokers']").Count);
        Assert.AreEqual(1, layout.FindAll("a[href='/Buyers']").Count);
        Assert.AreEqual(0, layout.FindAll("a[href='/OtherCompany']").Count);
    }

    private sealed class MenuFixture : IDisposable
    {
        private readonly Guid _companyId;
        private readonly TestAuthorizationContext _authorization;
        public Bunit.TestContext Context { get; } = new();

        public MenuFixture()
        {
            var factory = new TestDatabaseFactory();
            var company = TestData.Company();
            var otherCompany = TestData.Company(2);
            _companyId = company.Id;
            using var database = factory.CreateDbContext();
            database.Companies.AddRange(company, otherCompany);
            database.Modules.AddRange(
                new BlendModule { Id = 1, CompanyId = company.Id, Name = "Blend" },
                new BlendModule { Id = 2, CompanyId = otherCompany.Id, Name = "Other" });
            database.Roles.AddRange(
                new BlendIdentityRole("role-1", "Role 1", company.Id) { Id = "role-1" },
                new BlendIdentityRole("role-2", "Role 2", company.Id) { Id = "role-2" },
                new BlendIdentityRole("other-role", "Other role", otherCompany.Id) { Id = "other-role" });
            database.MenuItems.AddRange(
                new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Brokers", Path = "/Brokers" },
                new BlendMenuItem { Id = 2, CompanyId = company.Id, ModuleId = 1, Name = "Buyers", Path = "/Buyers" },
                new BlendMenuItem { Id = 3, CompanyId = otherCompany.Id, ModuleId = 2, Name = "Other", Path = "/OtherCompany" });
            database.MenuItemRoles.AddRange(
                new BlendMenuItemRole { MenuItemId = 1, RoleId = "role-1" },
                new BlendMenuItemRole { MenuItemId = 2, RoleId = "role-2" },
                new BlendMenuItemRole { MenuItemId = 3, RoleId = "other-role" });
            database.SaveChanges();
            Context.Services.AddSingleton<IDbContextFactory<BlendIdentityDbContext>>(factory);
            _authorization = Context.AddTestAuthorization();
        }

        public void Authorize(params string[] roles) => Authorize(false, roles);

        public void Authorize(bool admin, params string[] roles)
        {
            _authorization.SetAuthorized("test-user");
            if (admin) _authorization.SetRoles(BlendRoles.Admin);
            _authorization.SetClaims([new Claim(BlendClaims.CompanyId, _companyId.ToString()), .. roles.Select(role => new Claim(BlendClaims.RoleId, role))]);
        }

        public void Dispose() => Context.Dispose();
    }
}
