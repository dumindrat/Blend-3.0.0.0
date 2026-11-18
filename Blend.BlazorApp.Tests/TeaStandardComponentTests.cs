using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.TeaStandards;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class TeaStandardComponentTests
{
    [TestMethod]
    public void MissingWorkingCompanyDisplaysAccessMessage()
    {
        using var fixture = new Fixture(companySelected: false);

        var page = fixture.Context.RenderComponent<TeaStandards>();

        Assert.IsTrue(page.Markup.Contains("selected working company is unavailable", StringComparison.Ordinal));
        Assert.AreEqual(0, fixture.Service.Searches.Count);
    }

    [TestMethod]
    public void ReadOnlyUserCanViewStandardsButCannotEdit()
    {
        using var fixture = new Fixture(admin: false, companyNumber: 2);

        var page = fixture.Context.RenderComponent<TeaStandards>();

        Assert.IsTrue(page.Markup.Contains("no tea-standard write operations assigned", StringComparison.Ordinal));
        Assert.IsTrue(page.Markup.Contains("Premium", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").HasAttribute("disabled"));
        Assert.AreEqual(("all", 2), fixture.Service.Searches.Single());
    }

    [TestMethod]
    public void AdminCanCreateStandard()
    {
        using var fixture = new Fixture(companyNumber: 2);
        var page = fixture.Context.RenderComponent<TeaStandards>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").Click();
        Assert.AreEqual("standard-editor-heading", page.Find("dialog").GetAttribute("aria-labelledby"));
        Assert.AreEqual("Add standard", page.Find("dialog h2").TextContent.Trim());
        fixture.DialogModule.VerifyInvoke("show");
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Created);
        page.Find("#standard-name").Change("  New standard  ");
        page.Find("form").Submit();

        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        fixture.DialogModule.VerifyInvoke("close");
        Assert.IsNotNull(fixture.Service.Created);
        Assert.AreEqual("New standard", fixture.Service.Created.StandardName);
        Assert.AreEqual("test-user", fixture.Service.Created.User);
        Assert.AreEqual(2, fixture.Service.CreateCompanyId);
        Assert.IsTrue(page.Markup.Contains("Tea standard created.", StringComparison.Ordinal));
        CollectionAssert.AreEqual(new[] { ("all", 2), ("all", 2) }, fixture.Service.Searches);
    }

    [TestMethod]
    public void CancelAndEscapeCloseEditorAndResetForm()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<TeaStandards>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").Click();
        page.Find("#standard-name").Change("Discarded");
        page.Find("dialog button[aria-label='Close']").Click();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").Click();
        Assert.AreEqual(string.Empty, page.Find("#standard-name").GetAttribute("value"));
        page.InvokeAsync(() => page.Instance.CancelEditorFromDialogAsync()).GetAwaiter().GetResult();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        Assert.IsNull(fixture.Service.Created);
    }

    [TestMethod]
    public void FailedSaveShowsErrorInsideEditor()
    {
        using var fixture = new Fixture();
        fixture.Service.CreateResult = BusinessOperationResult.Failure("Creation refused.");
        var page = fixture.Context.RenderComponent<TeaStandards>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").Click();
        page.Find("#standard-name").Change("Standard");
        page.Find("form").Submit();

        Assert.IsTrue(page.Find("dialog [role='alert']").TextContent.Contains("Creation refused.", StringComparison.Ordinal));
        Assert.AreEqual(1, page.FindAll("dialog form").Count);
    }

    [TestMethod]
    public void SearchPassesWorkingCompanyMappingId()
    {
        using var fixture = new Fixture(companyNumber: 2);
        var page = fixture.Context.RenderComponent<TeaStandards>();

        page.Find("input[aria-label='Search tea standards']").Input(" Green ");
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Search").Click();

        CollectionAssert.AreEqual(new[] { ("all", 2), ("Green", 2) }, fixture.Service.Searches);
    }

    [TestMethod]
    public void EditAndDeleteUseWorkingCompanyForChecksAndRefresh()
    {
        using var fixture = new Fixture(companyNumber: 3);
        var page = fixture.Context.RenderComponent<TeaStandards>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Edit").Click();
        page.Find("#standard-name").Change("Updated");
        page.Find("form").Submit();
        Assert.AreEqual("Updated", fixture.Service.Updated?.StandardName);
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete standard").Click();

        Assert.AreEqual(3, fixture.Service.DeletedId);
        CollectionAssert.AreEqual(Enumerable.Repeat(("all", 3), 5).ToArray(), fixture.Service.Searches);
    }

    [TestMethod]
    public void ChangedCompanyMappingPreventsFurtherReadsAndWrites()
    {
        using var fixture = new Fixture(companyNumber: 2);
        var page = fixture.Context.RenderComponent<TeaStandards>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Edit").Click();
        fixture.ChangeCompanyNumber(3);

        page.Find("form").Submit();

        Assert.IsNull(fixture.Service.Updated);
        Assert.IsTrue(page.Markup.Contains("working company changed or is unavailable", StringComparison.Ordinal));
        Assert.AreEqual(1, fixture.Service.Searches.Count);
    }

    [TestMethod]
    public void ChangedCompanyMappingPreventsCreate()
    {
        using var fixture = new Fixture(companyNumber: 2);
        var page = fixture.Context.RenderComponent<TeaStandards>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add standard").Click();
        page.Find("#standard-name").Change("New standard");
        fixture.ChangeCompanyNumber(3);

        page.Find("form").Submit();

        Assert.IsNull(fixture.Service.Created);
        Assert.IsTrue(page.Markup.Contains("working company changed or is unavailable", StringComparison.Ordinal));
        Assert.AreEqual(1, fixture.Service.Searches.Count);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakeTeaStandardService Service { get; } = new();
        public BunitJSModuleInterop DialogModule { get; }

        public Fixture(bool admin = true, bool companySelected = true, int companyNumber = 1)
        {
            var company = TestData.Company(companyNumber);
            var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.Roles.Add(role);
            _database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Tea Standards", Path = "/TeaStandards" });
            _database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            _database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = role.Id });
            _database.SaveChanges();

            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<ITeaStandardService>(Service);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuthenticationStateProvider(TestData.Principal(companySelected ? company.Id : null, admin, role.Id)));
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            DialogModule = Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js");
            DialogModule.Mode = JSRuntimeMode.Loose;
        }

        public void ChangeCompanyNumber(int number)
        {
            _database.Companies.Single().CompanyNumber = number;
            _database.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
            _database.Dispose();
        }
    }

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class FakeTeaStandardService : ITeaStandardService
    {
        private readonly TeaStandard _standard = new() { StandardId = 3, StandardName = "Premium", RowVersion = [1] };
        public List<(string SearchText, int CompanyId)> Searches { get; } = [];
        public TeaStandard? Created { get; private set; }
        public int? CreateCompanyId { get; private set; }
        public TeaStandard? Updated { get; private set; }
        public int? DeletedId { get; private set; }
        public BusinessOperationResult CreateResult { get; set; } = BusinessOperationResult.Success();
        public IReadOnlyList<TeaStandard> GetAllStandards(string searchText, int companyId) { Searches.Add((searchText, companyId)); return [_standard]; }
        public BusinessOperationResult CreateStandard(TeaStandard standard, int companyId) { Created = standard; CreateCompanyId = companyId; return CreateResult; }
        public BusinessOperationResult UpdateStandard(TeaStandard standard) { Updated = standard; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteStandard(int standardId) { DeletedId = standardId; return BusinessOperationResult.Success(); }
    }
}
