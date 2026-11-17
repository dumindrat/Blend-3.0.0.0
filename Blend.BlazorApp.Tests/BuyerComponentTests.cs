using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Buyers;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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
        AssertButtons(page, false, false, false);
        Assert.AreEqual(("all", 1), fixture.Service.Searches.Single());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    public void EachOperationGrantEnablesOnlyItsButton(int grants)
    {
        using var fixture = new BuyerFixture(admin: false, grants: grants);
        var page = fixture.Context.RenderComponent<Buyers>();
        AssertButtons(page, (grants & 1) != 0, (grants & 2) != 0, (grants & 4) != 0);
    }

    [TestMethod]
    public void GrantsRequireMatchingMenuAndUserRoleAssignments()
    {
        using var noMenu = new BuyerFixture(admin: false, grants: 7, menuAssigned: false);
        AssertButtons(noMenu.Context.RenderComponent<Buyers>(), false, false, false);
        using var noUserRole = new BuyerFixture(admin: false, grants: 7, userRoleAssigned: false);
        AssertButtons(noUserRole.Context.RenderComponent<Buyers>(), false, false, false);
        using var wrongMenu = new BuyerFixture(admin: false, grants: 7, menuPath: "/Brokers");
        AssertButtons(wrongMenu.Context.RenderComponent<Buyers>(), false, false, false);
    }

    [TestMethod]
    public void RevokedPermissionIsRecheckedBeforeDeleting()
    {
        using var fixture = new BuyerFixture(admin: false, grants: 4);
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();
        fixture.RevokeGrant("delete");
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete buyer").Click();
        Assert.IsNull(fixture.Service.DeletedId);
        Assert.IsTrue(page.Markup.Contains("not permitted to delete buyers", StringComparison.Ordinal));
        AssertButtons(page, false, false, false);
    }

    [TestMethod]
    [DataRow("create")]
    [DataRow("edit")]
    public void RevokedPermissionIsRecheckedBeforeSaving(string operation)
    {
        using var fixture = new BuyerFixture(admin: false, grants: operation == "create" ? 1 : 2);
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == (operation == "create" ? "Add buyer" : "Edit")).Click();
        page.Find("#buyer-code").Change("ABC");
        page.Find("#buyer-name").Change("Example");
        fixture.RevokeGrant(operation);
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Created);
        Assert.IsNull(fixture.Service.Updated);
        Assert.IsTrue(page.Markup.Contains($"not permitted to {operation} buyers", StringComparison.Ordinal));
        AssertButtons(page, false, false, false);
    }

    private static void AssertButtons(IRenderedComponent<Buyers> page, bool canCreate, bool canEdit, bool canDelete)
    {
        foreach (var (label, allowed) in new[] { ("Add buyer", canCreate), ("Edit", canEdit), ("Delete", canDelete) })
        {
            var button = page.FindAll("button").Single(element => element.TextContent.Trim() == label);
            Assert.AreEqual(!allowed, button.HasAttribute("disabled"), $"Unexpected state for {label}.");
        }
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
    public void OptionalEmailCanBeBlankWhenCreatingOrEditing()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add buyer").Click();
        page.Find("#buyer-code").Change("B1");
        page.Find("#buyer-name").Change("Example");
        page.Find("#buyer-email").Change("buyer@example.com");
        page.Find("#buyer-email").Change("");
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Created, page.Markup);
        Assert.IsNull(fixture.Service.Created.Email);
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Edit").Click();
        page.Find("form").Submit();
        Assert.IsNotNull(fixture.Service.Updated, page.Markup);
        Assert.IsNull(fixture.Service.Updated.Email);
    }

    [TestMethod]
    public void InvalidEmailPreventsCreatingBuyer()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Add buyer").Click();
        page.Find("#buyer-code").Change("B1");
        page.Find("#buyer-name").Change("Example");
        page.Find("#buyer-email").Change("invalid");
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Created);
        Assert.IsTrue(page.Markup.Contains("valid e-mail address", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void SearchUsesWorkingCompanyAndEnteredText()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        Assert.IsTrue(page.Markup.Contains("Buyer One", StringComparison.Ordinal));
        page.Find("[aria-label='Search buyers']").Input(" Buyer ");
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Search").Click();
        Assert.AreEqual(("Buyer", 1), fixture.Service.Searches.Last());
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

    [TestMethod]
    public void DeleteConfirmsAndRefreshesDirectory()
    {
        using var fixture = new BuyerFixture();
        var page = fixture.Context.RenderComponent<Buyers>();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Delete buyer").Click();
        Assert.AreEqual(7, fixture.Service.DeletedId);
        Assert.IsTrue(page.Markup.Contains("Buyer deleted.", StringComparison.Ordinal));
        Assert.AreEqual(3, fixture.Service.Searches.Count);
    }

    private sealed class BuyerFixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakeBuyerService Service { get; } = new();

        public BuyerFixture(bool admin = true, bool companySelected = true, int grants = 0, bool menuAssigned = true, bool userRoleAssigned = true, string menuPath = "/Buyers")
        {
            var company = TestData.Company();
            var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.Roles.Add(role);
            _database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Buyers", Path = menuPath });
            if (menuAssigned) _database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            if (userRoleAssigned) _database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = role.Id });
            var keys = new[] { "create", "edit", "delete" };
            for (var index = 0; index < keys.Length; index++)
            {
                var operation = new BlendMenuItemOperation { Id = index + 1, MenuItemId = 1, Key = keys[index], Name = keys[index] };
                _database.MenuItemOperations.Add(operation);
                if ((grants & (1 << index)) != 0)
                    _database.RoleOperations.Add(new BlendRoleOperation { RoleId = role.Id, OperationId = operation.Id });
            }
            _database.SaveChanges();
            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<IBuyerService>(Service);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuthenticationStateProvider(TestData.Principal(companySelected ? company.Id : null, admin, role.Id)));
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js").Mode = JSRuntimeMode.Loose;
        }

        public void RevokeGrant(string key)
        {
            var operationId = _database.MenuItemOperations.Single(operation => operation.Key == key).Id;
            _database.RoleOperations.Remove(_database.RoleOperations.Single(grant => grant.OperationId == operationId));
            _database.SaveChanges();
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
