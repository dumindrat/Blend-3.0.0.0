using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Brokers;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class BrokerComponentTests
{
    [TestMethod]
    public void MissingWorkingCompanyDisplaysAccessMessage()
    {
        using var fixture = new BrokerFixture(companySelected: false);
        var page = fixture.Context.RenderComponent<Brokers>();
        Assert.IsTrue(page.Markup.Contains("selected working company is unavailable", StringComparison.Ordinal));
        Assert.AreEqual(0, fixture.Service.Searches.Count);
    }

    [TestMethod]
    public void UserWithoutWriteGrantsSeesReadOnlyDirectory()
    {
        using var fixture = new BrokerFixture(admin: false);
        var page = fixture.Context.RenderComponent<Brokers>();
        Assert.IsTrue(page.Markup.Contains("no broker write operations assigned", StringComparison.Ordinal));
        Assert.AreEqual(0, page.FindAll(".row-actions button").Count);
        Assert.IsFalse(page.FindAll("button").Any(button => button.TextContent.Trim() == "Add broker"));
        Assert.AreEqual(("all", 1), fixture.Service.Searches.Single());
    }

    [TestMethod]
    public void AdminCanCreateBrokerAndValidationPreventsEmptySubmission()
    {
        using var fixture = new BrokerFixture();
        var page = fixture.Context.RenderComponent<Brokers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add broker").Click();
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Created);
        Assert.IsTrue(page.Markup.Contains("Broker code field is required", StringComparison.Ordinal));
        page.Find("#broker-code").Change(" A ");
        page.Find("#broker-name").Change(" Example ");
        page.Find("#broker-email").Change("broker@example.com");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Created, page.Markup);
        Assert.AreEqual("A", fixture.Service.Created.BrokerCode);
        Assert.AreEqual("Example", fixture.Service.Created.BrokerName);
        Assert.AreEqual("broker@example.com", fixture.Service.Created.Email);
        Assert.AreEqual(1, fixture.Service.Created.CompanyId);
        Assert.IsTrue(page.Markup.Contains("Broker created.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EditUsesOriginalVersionAndMappedCompany()
    {
        using var fixture = new BrokerFixture();
        var page = fixture.Context.RenderComponent<Brokers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Edit").Click();
        page.Find("#broker-name").Change("Updated");
        page.Find("#broker-email").Change("broker@example.com");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Updated, page.Markup);
        Assert.AreEqual(7, fixture.Service.Updated.BrokerId);
        Assert.AreEqual(1, fixture.Service.Updated.CompanyId);
        Assert.AreEqual("Updated", fixture.Service.Updated.BrokerName);
        Assert.AreEqual("broker@example.com", fixture.Service.Updated.Email);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, fixture.Service.Updated.RowVersion);
        Assert.IsTrue(page.Markup.Contains("Broker updated.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void DeleteRequiresConfirmationAndPreservesFailureMessage()
    {
        using var fixture = new BrokerFixture();
        fixture.Service.DeleteResult = BusinessOperationResult.Failure("Deletion refused.");
        var page = fixture.Context.RenderComponent<Brokers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();
        Assert.IsTrue(page.Markup.Contains("Delete broker?", StringComparison.Ordinal));
        Assert.IsNull(fixture.Service.DeletedId);
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete broker").Click();
        Assert.AreEqual(7, fixture.Service.DeletedId);
        Assert.IsTrue(page.Markup.Contains("Deletion refused.", StringComparison.Ordinal));
        Assert.IsTrue(page.Markup.Contains("Delete broker?", StringComparison.Ordinal));
    }

    private sealed class BrokerFixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakeBrokerService Service { get; } = new();

        public BrokerFixture(bool admin = true, bool companySelected = true)
        {
            var company = TestData.Company();
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.SaveChanges();
            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<IBrokerService>(Service);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuthenticationStateProvider(TestData.Principal(companySelected ? company.Id : null, admin)));
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Brokers.razor.js").Mode = JSRuntimeMode.Loose;
        }

        public void Dispose() { Context.Dispose(); _database.Dispose(); }
    }

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class FakeBrokerService : IBrokerService
    {
        private readonly Broker _broker = new() { BrokerId = 7, BrokerCode = "ABC", BrokerName = "Broker One", CompanyId = 1, RowVersion = [1, 2] };
        public List<(string, int)> Searches { get; } = [];
        public Broker? Created { get; private set; }
        public Broker? Updated { get; private set; }
        public int? DeletedId { get; private set; }
        public BusinessOperationResult DeleteResult { get; set; } = BusinessOperationResult.Success();
        public Broker? GetBrokerById(int brokerId) => brokerId == _broker.BrokerId ? _broker : null;
        public IReadOnlyList<Broker> GetAllBrokers(string searchText, int companyId) { Searches.Add((searchText, companyId)); return [_broker]; }
        public BusinessOperationResult CreateBroker(Broker broker) { Created = broker; return BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateBroker(Broker broker) { Updated = broker; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteBroker(int brokerId) { DeletedId = brokerId; return DeleteResult; }
    }
}
