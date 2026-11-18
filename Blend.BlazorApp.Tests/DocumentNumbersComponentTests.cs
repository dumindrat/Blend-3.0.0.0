using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Documents;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class DocumentNumbersComponentTests
{
    [TestMethod]
    public void ListsOnlyWorkingCompanyAndDisablesResetWithoutPermission()
    {
        using var fixture = new Fixture(admin: false);
        var page = fixture.Context.RenderComponent<DocumentNumbers>();

        Assert.IsTrue(page.Markup.Contains("Company two", StringComparison.Ordinal));
        Assert.IsFalse(page.Markup.Contains("Other company", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").HasAttribute("disabled"));
    }

    [TestMethod]
    public void AdminCanResetAfterConfirmationWithWorkingCompany()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        Assert.AreEqual("Reset Company two", page.Find("dialog[aria-labelledby='document-reset-heading'] h2").TextContent.Trim());
        fixture.DialogModule.VerifyInvoke("show");
        page.Find("#document-number").Change("23");
        page.Find("#document-prefix").Change("/26");
        page.Find("form").Submit();
        Assert.IsNull(fixture.Service.Reset);
        Assert.IsTrue(page.Find("fieldset").HasAttribute("disabled"));
        var confirmation = page.Find("dialog[aria-labelledby='document-confirm-heading']");
        Assert.IsTrue(confirmation.TextContent.Contains("23", StringComparison.Ordinal));
        Assert.IsTrue(confirmation.TextContent.Contains("/26", StringComparison.Ordinal));
        Assert.IsTrue(confirmation.TextContent.Contains("0000", StringComparison.Ordinal));
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Confirm reset").Click();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        fixture.DialogModule.VerifyInvoke("close", 2);

        Assert.IsNotNull(fixture.Service.Reset);
        Assert.AreEqual(2, fixture.Service.Reset.CompanyId);
        Assert.AreEqual(3, fixture.Service.Reset.DocType);
        Assert.AreEqual(23, fixture.Service.Reset.DocNo);
        Assert.AreEqual("/26", fixture.Service.Reset.Prefix);
        Assert.AreEqual("0000", fixture.Service.Reset.DocFormat);
        Assert.AreEqual("test-user", fixture.Service.Reset.User);
        Assert.IsTrue(page.Markup.Contains("Document number reset.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void StaleNumberCannotBeReset()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("form").Submit();
        fixture.Service.CurrentNumber = 99;
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Confirm reset").Click();

        Assert.IsNull(fixture.Service.Reset);
        Assert.IsTrue(page.Markup.Contains("changed or is no longer available", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RevokedPermissionIsRecheckedBeforeReset()
    {
        using var fixture = new Fixture(admin: false, grant: true);
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("form").Submit();
        fixture.Revoke();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Confirm reset").Click();

        Assert.IsNull(fixture.Service.Reset);
        Assert.IsTrue(page.Markup.Contains("not permitted to reset", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ChangedCompanyMappingPreventsReset()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("form").Submit();
        fixture.ChangeCompanyNumber();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Confirm reset").Click();

        Assert.IsNull(fixture.Service.Reset);
        Assert.IsTrue(page.Markup.Contains("working company changed or is unavailable", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ResetFailureIsVisibleAndDoesNotReportSuccess()
    {
        using var fixture = new Fixture();
        fixture.Service.ResetResult = BusinessOperationResult.Failure("Reset refused.");
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("form").Submit();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Confirm reset").Click();

        Assert.IsTrue(page.Find("dialog[aria-labelledby='document-confirm-heading'] [role='alert']").TextContent.Contains("Reset refused.", StringComparison.Ordinal));
        Assert.IsFalse(page.Markup.Contains("Document number reset.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void InvalidFormCannotBeConfirmed()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("#document-prefix").Change("");
        page.Find("form").Submit();
        Assert.IsFalse(page.Markup.Contains("Confirm reset", StringComparison.Ordinal));
        Assert.IsNull(fixture.Service.Reset);
    }

    [TestMethod]
    public void BackAndEscapeDiscardConfirmationWithoutResetting()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("#document-number").Change("23");
        page.Find("form").Submit();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Back").Click();
        Assert.AreEqual(0, page.FindAll("dialog[aria-labelledby='document-confirm-heading'] h2").Count);
        Assert.IsFalse(page.Find("fieldset").HasAttribute("disabled"));
        page.Find("form").Submit();
        page.InvokeAsync(() => page.Instance.CancelConfirmationFromDialogAsync()).GetAwaiter().GetResult();
        Assert.AreEqual(0, page.FindAll("dialog[aria-labelledby='document-confirm-heading'] h2").Count);
        page.InvokeAsync(() => page.Instance.CancelEditorFromDialogAsync()).GetAwaiter().GetResult();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        Assert.IsNull(fixture.Service.Reset);
    }

    [TestMethod]
    public void CancelEditorDiscardsChanges()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        page.Find("#document-number").Change("23");
        page.Find("dialog[aria-labelledby='document-reset-heading'] button[aria-label='Close']").Click();
        Assert.AreEqual(0, page.FindAll("dialog form").Count);
        page.FindAll("button").Single(item => item.TextContent.Trim() == "Reset").Click();
        Assert.AreEqual("12", page.Find("#document-number").GetAttribute("value"));
        Assert.IsNull(fixture.Service.Reset);
    }

    [TestMethod]
    public void ListsOnlySelectedModuleAndCompany()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        Assert.IsFalse(page.Markup.Contains("Stores number", StringComparison.Ordinal));
        Assert.AreEqual((1, 2), fixture.Service.ModuleCalls.First());
    }

    [TestMethod]
    public void StoresModuleShowsStoresNumbers()
    {
        using var fixture = new Fixture();
        fixture.Context.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult("2");
        var page = fixture.Context.RenderComponent<DocumentNumbers>();
        Assert.IsTrue(page.Markup.Contains("Stores number", StringComparison.Ordinal));
        Assert.IsFalse(page.Markup.Contains("Company two", StringComparison.Ordinal));
        Assert.AreEqual((2, 2), fixture.Service.ModuleCalls.First());
    }

    private sealed class Fixture : IDisposable
    {
        private readonly BlendIdentityDbContext _database;
        public Bunit.TestContext Context { get; } = new();
        public FakeDocumentService Service { get; } = new();
        public BunitJSModuleInterop DialogModule { get; }

        public Fixture(bool admin = true, bool grant = false)
        {
            var company = TestData.Company(2);
            var role = new BlendIdentityRole("role", "Role", company.Id) { Id = "role-1" };
            _database = new TestDatabaseFactory().CreateDbContext();
            _database.Companies.Add(company);
            _database.Modules.Add(new BlendModule { Id = 1, CompanyId = company.Id, Name = "Blend" });
            _database.Modules.Add(new BlendModule { Id = 2, CompanyId = company.Id, Name = "Stores" });
            _database.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            _database.Roles.Add(role);
            _database.UserRoles.Add(new IdentityUserRole<string> { UserId = "user-1", RoleId = role.Id });
            _database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Document Numbers", Path = "/DocumentNumbers" });
            _database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            _database.MenuItemOperations.Add(new BlendMenuItemOperation { Id = 1, MenuItemId = 1, Key = "edit", Name = "Edit" });
            if (grant) _database.RoleOperations.Add(new BlendRoleOperation { RoleId = role.Id, OperationId = 1 });
            _database.SaveChanges();
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            DialogModule = Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js");
            DialogModule.Mode = JSRuntimeMode.Loose;
            Context.Services.AddSingleton(_database);
            Context.Services.AddSingleton<IDocumentService>(Service);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuthenticationStateProvider(TestData.Principal(company.Id, admin, role.Id)));
        }

        public void Revoke()
        {
            _database.RoleOperations.Remove(_database.RoleOperations.Single());
            _database.SaveChanges();
        }

        public void ChangeCompanyNumber()
        {
            _database.Companies.Single().CompanyNumber = 3;
            _database.SaveChanges();
        }

        public void Dispose() { Context.Dispose(); _database.Dispose(); }
    }

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class FakeDocumentService : IDocumentService
    {
        public int CurrentNumber { get; set; } = 12;
        public DocumentNo? Reset { get; private set; }
        public BusinessOperationResult ResetResult { get; set; } = BusinessOperationResult.Success();
        public IReadOnlyList<DocumentNo> GetDocumentData() => throw new NotSupportedException();
        public DocumentNo GetDocumentNo(int docType, int companyId) => throw new NotSupportedException();
        public IReadOnlyList<DocumentNo> GetDocumentDataByModule(int moduleId, int companyId)
        {
            ModuleCalls.Add((moduleId, companyId));
            return Rows().Where(item => item.ModuleId == moduleId && item.CompanyId == companyId).ToArray();
        }
        public List<(int ModuleId, int CompanyId)> ModuleCalls { get; } = [];
        private DocumentNo[] Rows() => [
            new() { Id = 1, CompanyId = 2, ModuleId = 1, DocType = 3, Description = "Company two", DocNo = CurrentNumber, Prefix = "P", DocFormat = "0000" },
            new() { Id = 2, CompanyId = 1, ModuleId = 1, DocType = 3, Description = "Other company", DocNo = 5, Prefix = "P", DocFormat = "0000" },
            new() { Id = 3, CompanyId = 2, ModuleId = 2, DocType = 7, Description = "Stores number", DocNo = 9, Prefix = "S", DocFormat = "0000" }
        ];
        public BusinessOperationResult ResetDocumentNo(DocumentNo documentInfo) { Reset = documentInfo; return ResetResult; }
        public bool IncrementDocumentNo(int docType) => throw new NotSupportedException();
    }
}
