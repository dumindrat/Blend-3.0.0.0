using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Brokers;
using Blend.BusinessService.Purchases;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class PurchaseReportsComponentTests
{
    [TestMethod]
    public void AssignedUserSeesCompanySalesAndDisabledReportActions()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        Assert.AreEqual(("all", 2), fixture.Purchases.Calls.Single());
        StringAssert.Contains(page.Markup, "2026-11-18");
        StringAssert.Contains(page.Markup, "2026-11-19");
        StringAssert.Contains(page.Markup, "Closed");
        StringAssert.Contains(page.Markup, "operator");
        StringAssert.Contains(page.Markup, "Report generation is not available yet");
        foreach (var label in new[] { "Advanced filter options", "Broker-wise purchases", "Sale Summary", "Purchase Sheet", "Canister Sheet" })
        {
            var buttons = page.FindAll("button").Where(b => b.TextContent.Trim() == label).ToArray();
            Assert.AreEqual(label is "Sale Summary" or "Purchase Sheet" or "Canister Sheet" ? 2 : 1, buttons.Length);
            Assert.IsTrue(buttons.All(button => button.HasAttribute("disabled")), label);
            Assert.IsTrue(buttons.All(button => button.GetAttribute("title")!.Contains("Print permission", StringComparison.Ordinal)), label);
        }
    }

    [TestMethod]
    public void PrintPermissionIsScopedToTheAssignedReportMenuAndRefreshed()
    {
        using var fixture = new Fixture();
        fixture.GrantOtherMenuPrint();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        Assert.IsTrue(page.FindAll("button").Where(b => b.HasAttribute("title")).All(b => b.GetAttribute("title")!.Contains("Print permission", StringComparison.Ordinal)));

        fixture.GrantPrint();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("button[title='Report generation is not available yet.']").Count));
        Assert.IsTrue(page.FindAll("button[title='Report generation is not available yet.']").All(b => b.HasAttribute("disabled")));
        Assert.IsFalse(page.FindAll("button").Single(b => b.TextContent.Trim() == "Advanced filter options").HasAttribute("disabled"));
        Assert.IsFalse(page.Markup.Contains("also requires", StringComparison.Ordinal));

        fixture.RevokePrint();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        page.WaitForAssertion(() => StringAssert.Contains(page.Markup, "also requires the Purchase Reports Print permission"));
        Assert.IsTrue(page.FindAll("button").Where(b => b.HasAttribute("title")).All(b => b.GetAttribute("title")!.Contains("Print permission", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void PrintGrantOnRoleWithoutReportMenuDoesNotGrantPrint()
    {
        using var fixture = new Fixture(secondRole: true);
        fixture.GrantUnassignedRolePrint();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        Assert.IsTrue(page.FindAll("button").Where(b => b.HasAttribute("title")).All(b => b.GetAttribute("title")!.Contains("Print permission", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void AuthorizedUserCanOpenAndCloseAllOptionDialogs()
    {
        using var fixture = new Fixture();
        fixture.GrantPrint();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        page.FindAll("button").First(b => b.TextContent.Trim() == "Sale Summary").Click();
        page.WaitForAssertion(() => StringAssert.Contains(page.Find("dialog").TextContent, "Sale Summary options"));
        StringAssert.Contains(page.Find("dialog").TextContent, "2026-11-18");
        Assert.IsTrue(page.Find("dialog input[type=checkbox]").HasAttribute("checked"));
        Assert.IsTrue(page.Find("dialog button[title='Report generation is not available yet']").HasAttribute("disabled"));
        Assert.AreEqual(2, page.FindAll("#report-output option").Count);
        page.Find("dialog button[aria-label='Close']").Click();

        page.FindAll("button").Last(b => b.TextContent.Trim() == "Purchase Sheet").Click();
        page.WaitForAssertion(() => StringAssert.Contains(page.Find("dialog").TextContent, "Purchase Sheet options"));
        StringAssert.Contains(page.Find("dialog").TextContent, "2026-11-19");
        Assert.IsFalse(page.Find("dialog input[type=checkbox]").HasAttribute("checked"));
        page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Cancel").Click();

        page.FindAll("button").Single(b => b.TextContent.Trim() == "Advanced filter options").Click();
        page.WaitForAssertion(() => Assert.AreEqual(1, page.FindAll("#report-grade").Count));
        foreach (var id in new[] { "report-from", "report-to", "report-grade", "report-standard", "report-sale-type", "report-broker-code", "report-output" }) Assert.IsNotNull(page.Find("#" + id));
        Assert.AreEqual("MS", page.Find("#report-sale-type").GetAttribute("value"));
        page.Find("#report-grade").Change("BOP");
        page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Cancel").Click();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Advanced filter options").Click();
        page.WaitForAssertion(() => Assert.AreEqual("", page.Find("#report-grade").GetAttribute("value")));
        page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Cancel").Click();

        page.FindAll("button").Single(b => b.TextContent.Trim() == "Broker-wise purchases").Click();
        page.WaitForAssertion(() => Assert.AreEqual(2, fixture.Brokers.LastCompany));
        page.WaitForAssertion(() => StringAssert.Contains(page.Find("dialog").TextContent, "Broker-wise purchases"));
        StringAssert.Contains(page.Find("#report-broker").TextContent, "Broker");
        Assert.IsNotNull(page.Find("#report-from"));
        Assert.IsNotNull(page.Find("#report-to"));
        Assert.IsTrue(page.Find("dialog button[title='Report generation is not available yet']").HasAttribute("disabled"));
        page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Cancel").Click();
        Assert.AreEqual(0, page.FindAll("#report-dialog-title").Count);
    }

    [TestMethod]
    public void BrokerOptionsShowLoadingAndHandleEmptyListAndEscape()
    {
        using var fixture = new Fixture();
        using var gate = new ManualResetEventSlim();
        fixture.GrantPrint();
        fixture.Brokers.Gate = gate;
        fixture.Brokers.Items = [];
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        try
        {
            page.FindAll("button").Single(b => b.TextContent.Trim() == "Broker-wise purchases").Click();
            page.WaitForAssertion(() =>
            {
                var button = page.Find("button[aria-busy='true']");
                Assert.IsTrue(button.HasAttribute("disabled"));
                StringAssert.Contains(button.TextContent, "Loading brokers");
                Assert.IsNotNull(button.QuerySelector(".spinner"));
            });
        }
        finally { gate.Set(); }
        page.WaitForAssertion(() => StringAssert.Contains(page.Find("dialog").TextContent, "No brokers found for this company"));
        page.InvokeAsync(() => page.Instance.CancelFromDialogAsync()).GetAwaiter().GetResult();
        page.WaitForAssertion(() => Assert.AreEqual(0, page.FindAll("#report-dialog-title").Count));
    }

    [TestMethod]
    public void RevokedPrintOrChangedCompanyPreventsOpeningOptions()
    {
        using var fixture = new Fixture();
        fixture.GrantPrint();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        fixture.RevokePrint();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Broker-wise purchases").Click();
        page.WaitForAssertion(() => StringAssert.Contains(page.Markup, "no longer has the Purchase Reports Print permission"));
        Assert.AreEqual(0, fixture.Brokers.LastCompany);
        Assert.AreEqual(0, page.FindAll("#report-dialog-title").Count);

        using var changed = new Fixture();
        changed.GrantPrint();
        var changedPage = changed.Context.RenderComponent<PurchaseReports>();
        changedPage.WaitForAssertion(() => Assert.AreEqual(2, changedPage.FindAll("tbody tr").Count));
        changed.ChangeCompanyMapping();
        changedPage.FindAll("button").First(b => b.TextContent.Trim() == "Sale Summary").Click();
        changedPage.WaitForAssertion(() => StringAssert.Contains(changedPage.Markup, "working company changed"));
        Assert.AreEqual(0, changedPage.FindAll("#report-dialog-title").Count);
    }

    [TestMethod]
    public void SearchUsesWorkingCompanyAndEmptySearchRequestsAll()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(1, fixture.Purchases.Calls.Count));
        page.Find("input[aria-label='Search sale date']").Input("  18/11  ");
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        page.WaitForAssertion(() => Assert.AreEqual(2, fixture.Purchases.Calls.Count));
        Assert.AreEqual(("18/11", 2), fixture.Purchases.Calls.Last());
        page.Find("input[aria-label='Search sale date']").Input("  ");
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        page.WaitForAssertion(() => Assert.AreEqual(3, fixture.Purchases.Calls.Count));
        Assert.AreEqual(("all", 2), fixture.Purchases.Calls.Last());
    }

    [TestMethod]
    public void MenuAndCompanyAreCheckedBeforeReadsAndRecheckedOnSearch()
    {
        using var noAccess = new Fixture();
        noAccess.RevokeMenu();
        var denied = noAccess.Context.RenderComponent<PurchaseReports>();
        denied.WaitForAssertion(() => StringAssert.Contains(denied.Markup, "does not have access"));
        Assert.AreEqual(0, noAccess.Purchases.Calls.Count);

        using var revoked = new Fixture();
        var page = revoked.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("tbody tr").Count));
        revoked.RevokeMenu();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        page.WaitForAssertion(() => StringAssert.Contains(page.Markup, "no longer has access"));
        Assert.AreEqual(1, revoked.Purchases.Calls.Count);
        Assert.AreEqual(0, page.FindAll("tbody tr").Count);

        using var changed = new Fixture();
        var changedPage = changed.Context.RenderComponent<PurchaseReports>();
        changedPage.WaitForAssertion(() => Assert.AreEqual(2, changedPage.FindAll("tbody tr").Count));
        changed.ChangeCompanyMapping();
        changedPage.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        changedPage.WaitForAssertion(() => StringAssert.Contains(changedPage.Markup, "working company changed"));
        Assert.AreEqual(1, changed.Purchases.Calls.Count);
        Assert.AreEqual(0, changedPage.FindAll("tbody tr").Count);

        using var removed = new Fixture();
        var removedPage = removed.Context.RenderComponent<PurchaseReports>();
        removedPage.WaitForAssertion(() => Assert.AreEqual(2, removedPage.FindAll("tbody tr").Count));
        removed.RemoveMembership();
        removedPage.FindAll("button").Single(b => b.TextContent.Trim() == "Search").Click();
        removedPage.WaitForAssertion(() => StringAssert.Contains(removedPage.Markup, "working company changed"));
        Assert.AreEqual(1, removed.Purchases.Calls.Count);
        Assert.AreEqual(0, removedPage.FindAll("tbody tr").Count);
    }

    [TestMethod]
    public void AdminCanViewAndUserWithoutMembershipCannotRead()
    {
        using var admin = new Fixture(admin: true);
        admin.RevokeMenu();
        var page = admin.Context.RenderComponent<PurchaseReports>();
        page.WaitForAssertion(() => Assert.AreEqual(1, admin.Purchases.Calls.Count));
        Assert.AreEqual(2, admin.Purchases.Calls.Single().Company);
        page.WaitForAssertion(() => Assert.AreEqual(2, page.FindAll("button[title='Report generation is not available yet.']").Count));
        Assert.IsTrue(page.FindAll("button[title='Report generation is not available yet.']").All(b => b.HasAttribute("disabled")));
        Assert.IsFalse(page.FindAll("button").Single(b => b.TextContent.Trim() == "Advanced filter options").HasAttribute("disabled"));

        using var noMembership = new Fixture();
        noMembership.RemoveMembership();
        var denied = noMembership.Context.RenderComponent<PurchaseReports>();
        denied.WaitForAssertion(() => StringAssert.Contains(denied.Markup, "Select an active working company"));
        Assert.AreEqual(0, noMembership.Purchases.Calls.Count);
    }

    [TestMethod]
    public void LoadingAndEmptyStatesAreVisible()
    {
        using var fixture = new Fixture();
        using var gate = new ManualResetEventSlim();
        fixture.Purchases.Gate = gate;
        fixture.Purchases.Results = [];
        var page = fixture.Context.RenderComponent<PurchaseReports>();
        try { page.WaitForAssertion(() => StringAssert.Contains(page.Markup, "Loading sales")); }
        finally { gate.Set(); }
        page.WaitForAssertion(() => StringAssert.Contains(page.Markup, "No sales found"));
        Assert.AreEqual(0, page.FindAll("tbody tr").Count);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakePurchases Purchases { get; } = new();
        public FakeBrokers Brokers { get; } = new();

        public Fixture(bool admin = false, bool secondRole = false)
        {
            var company = TestData.Company(2);
            var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.Modules.Add(new BlendModule { Id = 1, CompanyId = company.Id, Name = "Blend" });
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.Roles.Add(role);
            _database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = role.Id });
            _database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Purchase Reports", Path = "/Reports/Purchase" });
            _database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            _database.MenuItemOperations.Add(new BlendMenuItemOperation { Id = 1, MenuItemId = 1, Key = "print", Name = "Print" });
            if (secondRole)
            {
                _database.Roles.Add(new BlendIdentityRole("other", "Other", company.Id) { Id = "role-2" });
                _database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = "role-2" });
            }
            _database.SaveChanges();
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js").Mode = JSRuntimeMode.Loose;
            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<IPurchaseService>(Purchases);
            Context.Services.AddSingleton<IBrokerService>(Brokers);
            var principal = TestData.Principal(company.Id, admin, role.Id);
            if (secondRole) ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(BlendClaims.RoleId, "role-2"));
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuth(principal));
        }

        public void RevokeMenu() { _database.MenuItemRoles.Remove(_database.MenuItemRoles.Single(item => item.MenuItemId == 1)); _database.SaveChanges(); }
        public void GrantPrint() { _database.RoleOperations.Add(new BlendRoleOperation { RoleId = "role-1", OperationId = 1 }); _database.SaveChanges(); }
        public void GrantUnassignedRolePrint() { _database.RoleOperations.Add(new BlendRoleOperation { RoleId = "role-2", OperationId = 1 }); _database.SaveChanges(); }
        public void RevokePrint() { _database.RoleOperations.Remove(_database.RoleOperations.Single(item => item.OperationId == 1)); _database.SaveChanges(); }
        public void GrantOtherMenuPrint()
        {
            var company = _database.Companies.Single();
            _database.MenuItems.Add(new BlendMenuItem { Id = 2, CompanyId = company.Id, ModuleId = 1, Name = "Create Purchase", Path = "/Operations/Purchase" });
            _database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 2, RoleId = "role-1" });
            _database.MenuItemOperations.Add(new BlendMenuItemOperation { Id = 2, MenuItemId = 2, Key = "print", Name = "Print" });
            _database.RoleOperations.Add(new BlendRoleOperation { RoleId = "role-1", OperationId = 2 });
            _database.SaveChanges();
        }
        public void RemoveMembership() { _database.UserCompanies.Remove(_database.UserCompanies.Single()); _database.SaveChanges(); }
        public void ChangeCompanyMapping() { _database.Companies.Single().CompanyNumber = 3; _database.SaveChanges(); }
        public void Dispose() { Context.Dispose(); _database.Dispose(); }
    }

    private sealed class FixedAuth(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
    }

    private sealed class FakeBrokers : IBrokerService
    {
        public int LastCompany { get; private set; }
        public ManualResetEventSlim? Gate { get; set; }
        public IReadOnlyList<Broker> Items { get; set; } = [new() { BrokerId = 7, BrokerCode = "B", BrokerName = "Broker" }];
        public IReadOnlyList<Broker> GetAllBrokers(string searchText, int companyId)
        {
            if (Gate is not null && !Gate.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Timed out waiting for broker lookup.");
            LastCompany = companyId;
            return Items;
        }
        public Broker? GetBrokerById(int brokerId) => throw new NotSupportedException();
        public BusinessOperationResult CreateBroker(Broker broker) => throw new NotSupportedException();
        public BusinessOperationResult UpdateBroker(Broker broker) => throw new NotSupportedException();
        public BusinessOperationResult DeleteBroker(int brokerId) => throw new NotSupportedException();
    }

    private sealed class FakePurchases : IPurchaseService
    {
        public List<(string Search, int Company)> Calls { get; } = [];
        public ManualResetEventSlim? Gate { get; set; }
        public IReadOnlyList<PurchaseSale> Results { get; set; } = [
            new() { SaleId = 17, SaleDate = "2026-11-18", SaleStatus = "Open", ModifiedDate = "2026-11-20", ModifiedUser = "operator" },
            new() { SaleId = 18, SaleDate = "2026-11-19", SaleStatus = "Closed" }
        ];
        public IReadOnlyList<PurchaseSale> GetAllSales(string searchString, int companyId)
        {
            if (Gate is not null && !Gate.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Timed out waiting for the sale read.");
            Calls.Add((searchString, companyId));
            return Results;
        }
        public IReadOnlyList<StraightLine> GetAddedStraightLines(int saleId) => throw new NotSupportedException();
        public StraightLine? GetStraightLineByReservedNo(long straightLineId) => throw new NotSupportedException();
        public IReadOnlyList<TeaCertificate> GetTeaCertificates() => throw new NotSupportedException();
        public BusinessOperationResult CreateSale(PurchaseSale sale) => throw new NotSupportedException();
        public BusinessOperationResult UpdateSale(PurchaseSale sale) => throw new NotSupportedException();
        public BusinessOperationResult CloseSale(PurchaseSale sale) => throw new NotSupportedException();
        public BusinessOperationResult OpenSale(PurchaseSale sale) => throw new NotSupportedException();
        public BusinessOperationResult AddStraightLine(StraightLine line) => throw new NotSupportedException();
        public BusinessOperationResult UpdateStraightLine(StraightLine line) => throw new NotSupportedException();
        public BusinessOperationResult DeleteStraightLine(StraightLine line) => throw new NotSupportedException();
    }
}
