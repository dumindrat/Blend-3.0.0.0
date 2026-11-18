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
        Assert.IsTrue(layout.Find("details.sidebar-menu-group").HasAttribute("open"));
    }

    [TestMethod]
    public void MasterDataGroupContainsOnlyAuthorizedItemsAndStartsCollapsed()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1");
        var layout = fixture.Context.RenderComponent<MainLayout>();
        var group = layout.Find("details.sidebar-menu-group");

        Assert.IsFalse(group.HasAttribute("open"));
        Assert.AreEqual("Master data", group.QuerySelector("summary span:nth-child(2)")!.TextContent.Trim());
        Assert.AreEqual(1, group.QuerySelectorAll("a[href='/Brokers']").Length);
        Assert.AreEqual(1, group.QuerySelectorAll("a[href='/TeaStandards']").Length);
        Assert.AreEqual(0, group.QuerySelectorAll("a[href='/Buyers'], a[href='/DocumentNumbers']").Length);
        Assert.AreEqual(0, group.QuerySelectorAll("a[href='/Orders']").Length);
        Assert.AreEqual(1, layout.FindAll(".module-nav-group a[href='/Orders']").Count);
    }

    [TestMethod]
    public void MasterDataGroupsRemainScopedToTheirModules()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize(admin: true);
        var layout = fixture.Context.RenderComponent<MainLayout>();
        var blend = layout.Find("[data-module-group][data-module-id='1'] details.sidebar-menu-group");
        var stores = layout.Find("[data-module-group][data-module-id='3'] details.sidebar-menu-group");

        Assert.AreEqual(1, blend.QuerySelectorAll("a[href='/Brokers']").Length);
        Assert.AreEqual(1, blend.QuerySelectorAll("a[href='/Buyers']").Length);
        Assert.AreEqual(1, blend.QuerySelectorAll("a[href='/TeaStandards']").Length);
        Assert.AreEqual(0, blend.QuerySelectorAll("a[href='/DocumentNumbers']").Length);
        Assert.AreEqual(1, stores.QuerySelectorAll("a[href='/DocumentNumbers'][data-module-id='3']").Length);
    }

    [TestMethod]
    public void AdministrationGroupShowsOnlyGrantedLinks()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1");
        fixture.SetPolicies(BlendPermissions.ManageAdministrationPolicy, BlendPermissions.ManageUsersPolicy);
        var layout = fixture.Context.RenderComponent<MainLayout>();
        var group = layout.Find("details.administration-menu-group");

        Assert.IsFalse(group.HasAttribute("open"));
        Assert.AreEqual("Administration", group.QuerySelector("summary span:nth-child(2)")!.TextContent.Trim());
        Assert.AreEqual(1, group.QuerySelectorAll("a[href='/Admin/Users']").Length);
        Assert.AreEqual(0, group.QuerySelectorAll("a[href='/Admin/Roles'], a[href='/Admin/MenuItems'], a[href='/Admin/Companies']").Length);
        Assert.IsFalse(group.HasAttribute("data-module-id"));
    }

    [TestMethod]
    public void AdministratorSeesAllAdministrationLinksAndActiveGroupIsOpen()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize(admin: true);
        fixture.SetPolicies(BlendPermissions.ManageAdministrationPolicy, BlendPermissions.ManageUsersPolicy, BlendPermissions.ManageRolesPolicy, BlendPermissions.ManageMenuItemsPolicy);
        fixture.Context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/Admin/Roles");
        var layout = fixture.Context.RenderComponent<MainLayout>();
        var group = layout.Find("details.administration-menu-group");

        Assert.IsTrue(group.HasAttribute("open"));
        Assert.AreEqual(4, group.QuerySelectorAll("a").Length);
        Assert.IsTrue(group.QuerySelector("a[href='/Admin/Roles']")!.ClassList.Contains("active"));
    }

    [TestMethod]
    public void AdministrationGroupIsHiddenWithoutPermissions()
    {
        using var fixture = new MenuFixture();
        fixture.Authorize("role-1");
        var layout = fixture.Context.RenderComponent<MainLayout>();

        Assert.AreEqual(0, layout.FindAll("details.administration-menu-group").Count);
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
                new BlendModule { Id = 2, CompanyId = otherCompany.Id, Name = "Other" },
                new BlendModule { Id = 3, CompanyId = company.Id, Name = "Stores" });
            database.Roles.AddRange(
                new BlendIdentityRole("role-1", "Role 1", company.Id) { Id = "role-1" },
                new BlendIdentityRole("role-2", "Role 2", company.Id) { Id = "role-2" },
                new BlendIdentityRole("other-role", "Other role", otherCompany.Id) { Id = "other-role" });
            database.MenuItems.AddRange(
                new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Brokers", Path = "/Brokers" },
                new BlendMenuItem { Id = 2, CompanyId = company.Id, ModuleId = 1, Name = "Buyers", Path = "/Buyers" },
                new BlendMenuItem { Id = 3, CompanyId = otherCompany.Id, ModuleId = 2, Name = "Other", Path = "/OtherCompany" },
                new BlendMenuItem { Id = 4, CompanyId = company.Id, ModuleId = 1, Name = "Tea Standards", Path = "/TeaStandards", GroupName = "Directory" },
                new BlendMenuItem { Id = 5, CompanyId = company.Id, ModuleId = 3, Name = "Document Numbers", Path = "/DocumentNumbers" },
                new BlendMenuItem { Id = 6, CompanyId = company.Id, ModuleId = 1, Name = "Orders", Path = "/Orders", GroupName = "Master data" });
            database.MenuItemRoles.AddRange(
                new BlendMenuItemRole { MenuItemId = 1, RoleId = "role-1" },
                new BlendMenuItemRole { MenuItemId = 2, RoleId = "role-2" },
                new BlendMenuItemRole { MenuItemId = 3, RoleId = "other-role" },
                new BlendMenuItemRole { MenuItemId = 4, RoleId = "role-1" },
                new BlendMenuItemRole { MenuItemId = 5, RoleId = "role-2" },
                new BlendMenuItemRole { MenuItemId = 6, RoleId = "role-1" });
            database.SaveChanges();
            Context.Services.AddSingleton<IDbContextFactory<BlendIdentityDbContext>>(factory);
            _authorization = Context.AddTestAuthorization();
        }

        public void Authorize(params string[] roles) => Authorize(false, roles);

        public void SetPolicies(params string[] policies) => _authorization.SetPolicies(policies);

        public void Authorize(bool admin, params string[] roles)
        {
            _authorization.SetAuthorized("test-user");
            if (admin) _authorization.SetRoles(BlendRoles.Admin);
            _authorization.SetClaims([new Claim(BlendClaims.CompanyId, _companyId.ToString()), .. roles.Select(role => new Claim(BlendClaims.RoleId, role))]);
        }

        public void Dispose() => Context.Dispose();
    }
}
