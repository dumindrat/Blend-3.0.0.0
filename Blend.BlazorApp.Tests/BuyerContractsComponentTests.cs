using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Buyers;
using Blend.BusinessService.Contracts;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class BuyerContractsComponentTests
{
    [TestMethod]
    public void ListUsesWorkingCompanyAndDisablesUnauthorizedWrites()
    {
        using var fixture = new Fixture(admin: false);
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        Assert.AreEqual(2, fixture.Contracts.Company);
        Assert.IsTrue(page.Markup.Contains("Contract A", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Add contract").HasAttribute("disabled"));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Edit").HasAttribute("disabled"));
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Standards").Click();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        Assert.IsTrue(page.Markup.Contains("Added standards", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Update").HasAttribute("disabled"));
    }

    [TestMethod]
    public void MissingMenuAssignmentPreventsReadingContracts()
    {
        using var fixture = new Fixture(admin: false);
        fixture.RevokeMenu();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        Assert.AreEqual(0, fixture.Contracts.Company);
        Assert.IsTrue(page.Markup.Contains("does not have access", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CreatePassesBuyerCompanyAndUser()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Add contract").Click();
        page.Find("#contract-name").Change("New contract");
        page.Find("#contract-buyer").Change("7");
        page.Find("#contract-date").Change("2026-11-18");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Contracts.Created);
        Assert.AreEqual(2, fixture.Contracts.Created.CompanyId);
        Assert.AreEqual(7, fixture.Contracts.Created.BuyerId);
        Assert.AreEqual("2026-11-18", fixture.Contracts.Created.CompletionDate);
        Assert.AreEqual("test-user", fixture.Contracts.Created.User);
        Assert.IsTrue(page.Markup.Contains("Contract saved.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void StaleContractCannotBeUpdatedOrDeleted()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Edit").Click();
        fixture.Contracts.Version = [2];
        page.Find("form").Submit();
        Assert.IsNull(fixture.Contracts.Updated);
        Assert.IsTrue(page.Markup.Contains("contract changed", StringComparison.Ordinal));
        page.Find("dialog button[aria-label='Close']").Click();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete").Click();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete contract").Click();
        Assert.IsNull(fixture.Contracts.Deleted);
    }

    [TestMethod]
    public void EditAndDeleteUseOriginalContractVersion()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Edit").Click();
        page.Find("#contract-name").Change("Updated");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Contracts.Updated);
        Assert.AreEqual("Updated", fixture.Contracts.Updated.ContractName);
        CollectionAssert.AreEqual(new byte[] { 1 }, fixture.Contracts.Updated.RowVersion);
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete").Click();
        Assert.IsNull(fixture.Contracts.Deleted);
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete contract").Click();
        Assert.AreEqual(17L, fixture.Contracts.Deleted!.ContractId);
    }

    [TestMethod]
    public void ChangedCompanyPreventsWrite()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Edit").Click();
        fixture.ChangeCompany();
        page.Find("form").Submit();
        Assert.IsNull(fixture.Contracts.Updated);
        Assert.IsTrue(page.Markup.Contains("working company changed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RevokedPermissionPreventsDelete()
    {
        using var fixture = new Fixture(admin: false, grant: true);
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete").Click();
        fixture.Revoke();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Delete contract").Click();
        Assert.IsNull(fixture.Contracts.Deleted);
        Assert.IsTrue(page.Markup.Contains("not permitted", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AddAndUpdateStandardsUseCompanyAndVersion()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Standards").Click();
        Assert.AreEqual((17L, 2), fixture.Contracts.AvailableRequest);
        var headings = page.FindAll("dialog h3").Select(heading => heading.TextContent.Trim()).ToArray();
        CollectionAssert.AreEqual(new[] { "Add standard", "Added standards" }, headings);
        var dialog = page.Find("dialog");
        var elements = dialog.QuerySelectorAll("form, input[aria-label='Search available standards'], table");
        CollectionAssert.AreEqual(new[] { "FORM", "INPUT", "TABLE" }, elements.Select(element => element.TagName).ToArray());
        page.Find("#contract-standard").Change("9");
        page.Find("#contract-quantity").Change("12.50");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Contracts.DetailCreated);
        Assert.AreEqual(17L, fixture.Contracts.DetailCreated.ContractId);
        Assert.AreEqual(9, fixture.Contracts.DetailCreated.StandardId);
        Assert.AreEqual(12.50m, fixture.Contracts.DetailCreated.OrderQty);
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Update").Click();
        page.Find("#contract-quantity").Change("42");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Contracts.DetailUpdated);
        Assert.AreEqual(42m, fixture.Contracts.DetailUpdated.OrderQty);
        CollectionAssert.AreEqual(new byte[] { 4 }, fixture.Contracts.DetailUpdated.RowVersion);
    }

    [TestMethod]
    public void StandardWriteRejectsExcessPrecision()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<BuyerContracts>();
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Standards").Click();
        page.Find("#contract-standard").Change("9");
        page.Find("#contract-quantity").Change("12.345");
        page.Find("form").Submit();
        Assert.IsNull(fixture.Contracts.DetailCreated);
        Assert.IsTrue(page.Markup.Contains("two decimal places", StringComparison.Ordinal));
    }

    private sealed class Fixture : IDisposable
    {
        private readonly BlendIdentityDbContext database;
        public Bunit.TestContext Context { get; } = new();
        public FakeContracts Contracts { get; } = new();
        public Fixture(bool admin = true, bool grant = false)
        {
            var company = TestData.Company(2);
            var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
            database = new TestDatabaseFactory().CreateDbContext();
            database.Companies.Add(company);
            database.Modules.Add(new BlendModule { Id = 1, CompanyId = company.Id, Name = "Blend" });
            database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            database.Roles.Add(role);
            database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = role.Id });
            database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Buyer Contracts", Path = "/BuyerContracts" });
            database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            database.MenuItemOperations.Add(new BlendMenuItemOperation { Id = 1, MenuItemId = 1, Key = "delete", Name = "Delete" });
            if (grant) database.RoleOperations.Add(new BlendRoleOperation { RoleId = role.Id, OperationId = 1 });
            database.SaveChanges();
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js").Mode = JSRuntimeMode.Loose;
            Context.Services.AddSingleton(database);
            Context.Services.AddSingleton<IContractService>(Contracts);
            Context.Services.AddSingleton<IBuyerService>(new FakeBuyers());
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuth(TestData.Principal(company.Id, admin, role.Id)));
        }
        public void Revoke() { database.RoleOperations.Remove(database.RoleOperations.Single()); database.SaveChanges(); }
        public void RevokeMenu() { database.MenuItemRoles.Remove(database.MenuItemRoles.Single()); database.SaveChanges(); }
        public void ChangeCompany() { database.Companies.Single().CompanyNumber = 3; database.SaveChanges(); }
        public void Dispose() { Context.Dispose(); database.Dispose(); }
    }
    private sealed class FixedAuth(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
    }
    private sealed class FakeBuyers : IBuyerService
    {
        public IReadOnlyList<Buyer> GetAllBuyers(string searchText, int companyId) => companyId == 2 ? [new Buyer { BuyerId = 7, BuyerCode = "B", BuyerName = "Buyer" }] : [];
        public BusinessOperationResult CreateBuyer(Buyer buyer) => throw new NotSupportedException();
        public BusinessOperationResult UpdateBuyer(Buyer buyer) => throw new NotSupportedException();
        public BusinessOperationResult DeleteBuyer(int buyerId) => throw new NotSupportedException();
    }
    private sealed class FakeContracts : IContractService
    {
        public int Company { get; private set; }
        public byte[] Version { get; set; } = [1];
        public BuyerContract? Created { get; private set; }
        public BuyerContract? Updated { get; private set; }
        public BuyerContract? Deleted { get; private set; }
        public BuyerContractDetail? DetailCreated { get; private set; }
        public BuyerContractDetail? DetailUpdated { get; private set; }
        public (long, int) AvailableRequest { get; private set; }
        public IReadOnlyList<BuyerContract> GetContractList(string searchText, int companyId) { Company = companyId; return [new BuyerContract { ContractId = 17, CompanyId = companyId, ContractName = "Contract A", BuyerId = 7, BuyerCode = "B", CompletionDate = "2026-11-18", RowVersion = Version }]; }
        public IReadOnlyList<BuyerContract> GetOpenContractList(int companyId) => throw new NotSupportedException();
        public BusinessOperationResult CreateContract(BuyerContract contract) { Created = contract; return BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateContract(BuyerContract contract) { Updated = contract; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteContract(BuyerContract contract) { Deleted = contract; return BusinessOperationResult.Success(); }
        public BuyerContract GetContractById(int id) => throw new NotSupportedException();
        public IReadOnlyList<BuyerContractDetail> GetBuyerContractDetailById(long contractId) => [new BuyerContractDetail { ContractId = 17, StandardId = 8, StandardName = "Existing", OrderQty = 5, RowVersion = [4] }];
        public IReadOnlyList<TeaStandardForContract> GetAvailableStandardsById(long contractId) => throw new NotSupportedException();
        public IReadOnlyList<TeaStandardForContract> GetAvailableStandardsByIdAndSearchStr(long contractId, string searchText, int companyId) { AvailableRequest = (contractId, companyId); return [new TeaStandardForContract { ContractId = 17, StandardId = 9, StandardName = "Available" }]; }
        public BusinessOperationResult CreateBuyerContractDetail(BuyerContractDetail detail) { DetailCreated = detail; return BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateBuyerContractDetail(BuyerContractDetail detail) { DetailUpdated = detail; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteBuyerContractDetail(BuyerContractDetail detail) => throw new NotSupportedException();
        public IReadOnlyList<BlendNoByCon> GetBlendListByConAndStanList(int contractId, int standardId) => throw new NotSupportedException();
    }
}
