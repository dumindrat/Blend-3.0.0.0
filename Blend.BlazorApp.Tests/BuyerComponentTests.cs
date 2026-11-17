using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Buyers;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class BuyerComponentTests
{
    [TestMethod]
    public void MissingWorkingCompanyDisplaysAccessMessage()
    {
        using var fixture = new BuyerFixture(companySelected: false);
        var page = fixture.Context.RenderComponent<Buyers>();
        Assert.IsTrue(page.Markup.Contains("selected working company is unavailable", StringComparison.Ordinal));
        Assert.AreEqual(0, fixture.Service.Searches.Count);
    }

    [TestMethod]
    public void UserWithoutWriteGrantsSeesReadOnlyDirectory()
    {
        using var fixture = new BuyerFixture(admin: false);
        var page = fixture.Context.RenderComponent<Buyers>();
        Assert.IsTrue(page.Markup.Contains("no buyer write operations assigned", StringComparison.Ordinal));
        Assert.AreEqual(0, page.FindAll(".row-actions button").Count);
        Assert.AreEqual(("all", 1), fixture.Service.Searches.Single());
    }

    [TestMethod]
    public void AdminCanCreateBuyerAfterValidation()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add buyer").Click();
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Created);
        Assert.IsTrue(page.Markup.Contains("Buyer code field is required", StringComparison.Ordinal));
        page.Find("#buyer-code").Change(" B1 ");
        page.Find("#buyer-name").Change(" Example ");
        page.Find("#buyer-email").Change("buyer@example.com");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Created, page.Markup);
        Assert.AreEqual("B1", fixture.Service.Created.BuyerCode);
        Assert.AreEqual("Example", fixture.Service.Created.BuyerName);
        Assert.AreEqual(1, fixture.Service.Created.CompanyId);
        Assert.AreEqual("test-user", fixture.Service.Created.User);
        Assert.IsTrue(page.Markup.Contains("Buyer created.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EditUsesOriginalVersionAndMappedCompany()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Edit").Click();
        page.Find("#buyer-name").Change("Updated");
        page.Find("#buyer-email").Change("buyer@example.com");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Updated, page.Markup);
        Assert.AreEqual(7, fixture.Service.Updated.BuyerId);
        Assert.AreEqual(1, fixture.Service.Updated.CompanyId);
        Assert.AreEqual("Updated", fixture.Service.Updated.BuyerName);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, fixture.Service.Updated.RowVersion);
        Assert.IsTrue(page.Markup.Contains("Buyer updated.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void DeleteRequiresConfirmationAndShowsFailureMessage()
    {
        using var fixture = new BuyerFixture();
        fixture.Service.DeleteResult = BusinessOperationResult.Failure("Deletion refused.");
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();
        Assert.IsTrue(page.Markup.Contains("Delete buyer?", StringComparison.Ordinal));
        Assert.IsNull(fixture.Service.DeletedId);
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete buyer").Click();
        Assert.AreEqual(7, fixture.Service.DeletedId);
        Assert.IsTrue(page.Markup.Contains("Deletion refused.", StringComparison.Ordinal));
    }

    private sealed class BuyerFixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakeBuyerService Service { get; } = new();

        public BuyerFixture(bool admin = true, bool companySelected = true)
        {
            var company = TestData.Company();
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.SaveChanges();
            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<IBuyerService>(Service);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuthenticationStateProvider(TestData.Principal(companySelected ? company.Id : null, admin)));
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js").Mode = JSRuntimeMode.Loose;
        }

        public void Dispose() { Context.Dispose(); _database.Dispose(); }
    }

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class FakeBuyerService : IBuyerService
    {
        private readonly Buyer _buyer = new() { BuyerId = 7, BuyerCode = "ABC", BuyerName = "Buyer One", CompanyId = 1, RowVersion = [1, 2] };
        public List<(string, int)> Searches { get; } = [];
        public Buyer? Created { get; private set; }
        public Buyer? Updated { get; private set; }
        public int? DeletedId { get; private set; }
        public BusinessOperationResult DeleteResult { get; set; } = BusinessOperationResult.Success();
        public IReadOnlyList<Buyer> GetAllBuyers(string searchText, int companyId) { Searches.Add((searchText, companyId)); return [_buyer]; }
        public BusinessOperationResult CreateBuyer(Buyer buyer) { Created = buyer; return BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateBuyer(Buyer buyer) { Updated = buyer; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteBuyer(int buyerId) { DeletedId = buyerId; return DeleteResult; }
    }
}
