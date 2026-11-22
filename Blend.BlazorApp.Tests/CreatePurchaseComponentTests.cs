using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components.Pages;
using Blend.BusinessService;
using Blend.BusinessService.Brokers;
using Blend.BusinessService.Documents;
using Blend.BusinessService.GardenMarks;
using Blend.BusinessService.Grades;
using Blend.BusinessService.Purchases;
using Blend.DataService.Purchases;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class CreatePurchaseComponentTests
{
    private static void Click(IRenderedComponent<CreatePurchase> page, string text)
    {
        page.FindAll("button").Single(button => button.TextContent.Trim() == text)
            .ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs()).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void RouteAndCompanyScopedListRespectMenuAndOperationGrants()
    {
        Assert.IsTrue(typeof(CreatePurchase).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Any(r => r.Template == "/Operations/Purchase"));
        using var fixture = new Fixture(admin: false);
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Assert.AreEqual(2, fixture.Purchases.LastCompany);
        Assert.IsTrue(page.Markup.Contains("2026-11-18", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Create sale").HasAttribute("disabled"));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Edit").HasAttribute("disabled"));
    }

    [TestMethod]
    public void MissingMenuPreventsSaleRead()
    {
        using var fixture = new Fixture(admin: false);
        fixture.RevokeMenu();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Assert.AreEqual(0, fixture.Purchases.LastCompany);
        Assert.IsTrue(page.Markup.Contains("does not have access", StringComparison.Ordinal));
    }

    [TestMethod]
    public void SaleCreateAndStatusRequireConfirmationAndPassCompany()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Create sale");
        Assert.IsTrue(page.Find("dialog").ClassList.Contains("purchase-sale-dialog"));
        page.Find("#sale-date").Change("2026-12-01");
        page.Find("form").Submit();
        Assert.AreEqual("2026-12-01", fixture.Purchases.Created!.SaleDate);
        Assert.AreEqual(2, fixture.Purchases.Created.CompanyId);
        Assert.AreEqual("test-user", fixture.Purchases.Created.User);
        Click(page, "Close sale");
        Assert.IsFalse(page.Find("dialog").ClassList.Contains("purchase-sale-dialog"));
        Assert.IsNull(fixture.Purchases.Closed);
        Click(page, "Confirm");
        Assert.AreEqual(17, fixture.Purchases.Closed!.SaleId);
    }

    [TestMethod]
    public void RevokedGrantOrChangedCompanyBlocksSaleWrite()
    {
        using var fixture = new Fixture(admin: false, grant: true);
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Create sale");
        fixture.RevokeCreate();
        page.Find("form").Submit();
        Assert.IsNull(fixture.Purchases.Created);
        Assert.IsTrue(page.Markup.Contains("not permitted", StringComparison.Ordinal));

        using var other = new Fixture();
        var otherPage = other.Context.RenderComponent<CreatePurchase>();
        Click(otherPage, "Create sale");
        other.ChangeCompany();
        otherPage.Find("form").Submit();
        Assert.IsNull(other.Purchases.Created);
        Assert.IsTrue(otherPage.Markup.Contains("working company changed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void LinesButtonShowsLoadingUntilTheReadCompletes()
    {
        using var fixture = new Fixture();
        using var release = new ManualResetEventSlim();
        fixture.Purchases.LineReadGate = release;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        var linesButton = page.FindAll("button").Single(b => b.TextContent.Trim() == "Lines");
        Assert.IsTrue(linesButton.ClassList.Contains("purchase-lines-button"));
        Assert.IsTrue(page.Find("th.purchase-sale-actions").TextContent.Contains("Actions", StringComparison.Ordinal));
        Assert.AreEqual(3, page.FindAll("td.purchase-sale-actions button").Count);
        try
        {
            linesButton.Click();
            page.WaitForAssertion(() =>
            {
                var button = page.Find("button[aria-busy='true']");
                Assert.IsTrue(button.ClassList.Contains("purchase-lines-button"));
                Assert.IsTrue(button.HasAttribute("disabled"));
                Assert.IsTrue(button.TextContent.Contains("Loading lines", StringComparison.Ordinal));
                Assert.IsNotNull(button.QuerySelector(".spinner"));
                Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Create sale").HasAttribute("disabled"));
            });
        }
        finally { release.Set(); }
        page.WaitForAssertion(() => Assert.IsTrue(page.Find("dialog").ClassList.Contains("purchase-lines-dialog")));
        Assert.AreEqual(0, page.FindAll("button[aria-busy='true']").Count);
    }

    [TestMethod]
    public void StaleSaleClearsLoadingAndReportsError()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        fixture.Purchases.IsClosed = true;
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Lines").Click();
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("sale changed", StringComparison.Ordinal)));
        Assert.IsFalse(page.FindAll("button").Single(b => b.TextContent.Trim() == "Lines").HasAttribute("disabled"));
        Assert.AreEqual(0, page.FindAll("button[aria-busy='true']").Count);
        Assert.AreEqual(0, page.FindAll("dialog.purchase-lines-dialog").Count);
    }

    [TestMethod]
    public void LinesDialogUsesWideLayoutOnlyForTheLineList()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Assert.IsTrue(page.Find("dialog").ClassList.Contains("purchase-lines-dialog"));
        Assert.AreEqual(14, page.FindAll("dialog table th").Count);
        Assert.AreEqual("Actions", page.Find("dialog th.purchase-line-actions").TextContent.Trim());
        Assert.AreEqual(2, page.FindAll("dialog td.purchase-line-actions button").Count);
        Click(page, "Add line");
        Assert.IsFalse(page.Find("dialog").ClassList.Contains("purchase-lines-dialog"));
        Click(page, "Cancel");
        Assert.IsTrue(page.Find("dialog").ClassList.Contains("purchase-lines-dialog"));
    }

    [TestMethod]
    public void PurchaseGridsFormatAmountsAndPreserveBlankRepeatedValues()
    {
        using var fixture = new Fixture();
        fixture.Purchases.DisplayLines = [
            new() { Id = 21, BoxNo = "BOX", NetWeight = 1234.5m, Packs = 2000, ReducedQty = 1200.25m, VirtualWeight = 1234567.89m, Rate = 2500.5m, VirtualValue = 12345678.9m },
            new() { Id = 22, IsRepeated = true, NetWeight = 12.5m, Packs = 0 }
        ];
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        var cells = page.Find("dialog tbody tr:first-child").QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToArray();
        CollectionAssert.AreEqual(new[] { "1,234.50", "2,000.00", "1,200.25", "1,234,567.89", "2,500.50", "12,345,678.90" }, cells.Skip(7).Take(6).ToArray());
        var repeated = page.Find("dialog tbody tr:nth-child(2)").QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToArray();
        CollectionAssert.AreEqual(new[] { "12.50", "0.00", "", "", "", "" }, repeated.Skip(7).Take(6).ToArray());

        Click(page, "Upload CSV");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("header\n0,LOT,2,MARK,INV,BOP,2000,2,8,3000,10,11,1200,13\n", "purchase.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("1 validated rows", StringComparison.Ordinal)));
        var preview = page.FindAll("dialog tbody tr").Single().QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToArray();
        CollectionAssert.AreEqual(new[] { "2,000.00", "2.00", "1,000.00", "3,000.00", "1,200.00" }, preview.Skip(5).ToArray());
    }

    [TestMethod]
    public void LineEditPreservesRepeatedVersionsAndCertifications()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Edit line");
        page.Find("#line-rate").Change("4");
        page.Find("form").Submit();
        var line = fixture.Purchases.UpdatedLine!;
        Assert.AreEqual(17, line.SaleId);
        Assert.AreEqual(2, line.CompanyId);
        Assert.AreEqual(22L, line.RepeatingId);
        CollectionAssert.AreEqual(new byte[] { 1 }, line.RowVersion1);
        CollectionAssert.AreEqual(new byte[] { 2 }, line.RowVersion2);
        Assert.AreEqual("Organic", line.TeaCertificates);
        Assert.IsTrue(page.Find("dialog").TextContent.Contains("Line saved.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AddLineShowsLoadingWhileDocumentNumberIsFetched()
    {
        using var fixture = new Fixture();
        using var release = new ManualResetEventSlim();
        fixture.Documents.ReadGate = release;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        try
        {
            page.FindAll("button").Single(b => b.TextContent.Trim() == "Add line").Click();
            page.WaitForAssertion(() =>
            {
                var button = page.Find("button.purchase-add-line-button[aria-busy='true']");
                Assert.IsTrue(button.HasAttribute("disabled"));
                Assert.IsNotNull(button.QuerySelector(".spinner"));
                Assert.IsTrue(button.TextContent.Contains("Loading", StringComparison.Ordinal));
                Assert.IsTrue(page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Upload CSV").HasAttribute("disabled"));
                Assert.IsTrue(page.FindAll("dialog button").Single(b => b.TextContent.Trim() == "Close").HasAttribute("disabled"));
            });
        }
        finally { release.Set(); }
        page.WaitForAssertion(() => Assert.IsNotNull(page.Find("#line-type")));
        Assert.AreEqual("0042P", page.Find(".purchase-document-preview strong").TextContent);
    }

    [TestMethod]
    public void AddLineShowsCurrentCompanyDocumentNumberAndRefreshesOnReopen()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Add line");
        Assert.AreEqual((1, 2), fixture.Documents.LastRequest);
        Assert.AreEqual("0042P", page.Find(".purchase-document-preview strong").TextContent);
        Assert.IsTrue(page.Find(".purchase-document-preview").TextContent.Contains("may change before saving", StringComparison.Ordinal));
        Click(page, "Cancel");
        fixture.Documents.Number = "0043P";
        Click(page, "Add line");
        Assert.AreEqual("0043P", page.Find(".purchase-document-preview strong").TextContent);
        Assert.AreEqual(2, fixture.Documents.Calls);
        Click(page, "Cancel");
        Click(page, "Edit line");
        Assert.IsFalse(page.Find("dialog").TextContent.Contains("Next document number", StringComparison.Ordinal));
    }

    [TestMethod]
    public void MissingDocumentNumberShowsErrorAndDoesNotOpenForm()
    {
        using var fixture = new Fixture();
        fixture.Documents.Fail = true;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        page.FindAll("button").Single(b => b.TextContent.Trim() == "Add line").Click();
        page.WaitForAssertion(() => Assert.IsTrue(page.Find("dialog").TextContent.Contains("document number is unavailable", StringComparison.Ordinal)));
        Assert.IsFalse(page.Find(".purchase-add-line-button").HasAttribute("disabled"));
        Assert.AreEqual(0, page.FindAll("#line-type").Count);
        fixture.Documents.Fail = false;
        Click(page, "Add line");
        Assert.IsTrue(page.Find("dialog").TextContent.Contains("0042P", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AddLinePassesValuesAndSelectedCertificate()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Add line");
        page.Find("#line-type").Change("PS");
        page.Find("#line-code").Change("001");
        page.Find("#line-lot").Change("LOT");
        page.Find("#line-invoice").Change("INV");
        page.Find("#line-broker").Change("7");
        page.Find("#line-mark").Change("MARK");
        page.Find("#line-grade").Change("BOP");
        page.Find("#line-packs1").Change("10");
        page.Find("#line-net1").Change("5");
        page.Find("#line-rate").Change("2");
        page.Find("dialog input[type=checkbox]").Change(true);
        page.Find("form").Submit();
        var line = fixture.Purchases.Added.Single();
        Assert.AreEqual(2, line.CompanyId);
        Assert.AreEqual(17, line.SaleId);
        Assert.AreEqual(7, line.BrokerId);
        Assert.AreEqual("Organic", line.TeaCertificates);
    }

    [TestMethod]
    public void RejectsExcessPrecisionInIndividualRepeatedLine()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Edit line");
        page.Find("#line-packs1").Change("1.01");
        page.Find("#line-net1").Change("1.01");
        page.Find("#line-packs2").Change("1.99");
        page.Find("#line-net2").Change("1.01");
        page.Find("#line-reduction").Change("0");
        page.Find("form").Submit();
        Assert.IsNull(fixture.Purchases.UpdatedLine);
        Assert.IsTrue(page.Markup.Contains("Calculated weight or value is invalid", StringComparison.Ordinal));
    }

    [TestMethod]
    public void SaleChangeBlocksLineDelete()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Delete line");
        fixture.Purchases.IsClosed = true;
        Click(page, "Delete line");
        Assert.IsNull(fixture.Purchases.DeletedLine);
        Assert.IsTrue(page.Markup.Contains("sale changed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void UploadShowsPartialResultsAndLogsWithoutRetrying()
    {
        using var fixture = new Fixture();
        fixture.Purchases.FailSecond = true;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Upload CSV");
        page.Find("#upload-broker").Change("7");
        page.Find("#upload-type").Change("MS");
        page.Find("#upload-code").Change("001");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("header\n0,LOT1,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n0,LOT2,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n", "purchase.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("2 validated rows", StringComparison.Ordinal)));
        Click(page, "Review import");
        Assert.AreEqual(0, fixture.Purchases.Added.Count);
        Click(page, "Confirm import");
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("1 saved, 1 not confirmed", StringComparison.Ordinal)));
        Assert.AreEqual(2, fixture.Purchases.Added.Count);
        Assert.AreEqual(2, fixture.Log.Value!.TotalRecords);
        Assert.AreEqual(1, fixture.Log.Value.ErrorRecords);
        using var audit = System.Text.Json.JsonDocument.Parse(fixture.Log.Value.UploadData!);
        Assert.AreEqual("LOT2", audit.RootElement[1].GetProperty("LotNo").GetString());
        Assert.AreEqual("INV", audit.RootElement[1].GetProperty("InvNo").GetString());
        Assert.AreEqual("96", audit.RootElement[1].GetProperty("Price").GetString());
        Assert.AreEqual("Rejected", audit.RootElement[1].GetProperty("Status").GetString());
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Review import").HasAttribute("disabled"));
    }

    [TestMethod]
    public void ImportStopsAndAuditsRemainingRowsWhenSaleCloses()
    {
        using var fixture = new Fixture();
        fixture.Purchases.CloseAfterFirst = true;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Upload CSV");
        page.Find("#upload-broker").Change("7");
        page.Find("#upload-type").Change("MS");
        page.Find("#upload-code").Change("001");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("header\n0,LOT1,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n0,LOT2,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n", "purchase.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("2 validated rows", StringComparison.Ordinal)));
        Click(page, "Review import");
        Click(page, "Confirm import");
        Assert.AreEqual(1, fixture.Purchases.Added.Count);
        Assert.AreEqual(1, fixture.Log.Value!.ErrorRecords);
        Assert.IsTrue(page.Markup.Contains("Not attempted", StringComparison.Ordinal));
        Assert.IsTrue(page.Markup.Contains("sale changed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AuditFailureDoesNotHideSavedRowsOrAllowRetry()
    {
        using var fixture = new Fixture();
        fixture.Log.Fail = true;
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Upload CSV");
        page.Find("#upload-broker").Change("7");
        page.Find("#upload-type").Change("MS");
        page.Find("#upload-code").Change("001");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("header\n0,LOT,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n", "purchase.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("1 validated rows", StringComparison.Ordinal)));
        Click(page, "Review import");
        Click(page, "Confirm import");
        Assert.AreEqual(1, fixture.Purchases.Added.Count);
        Assert.IsTrue(page.Markup.Contains("1 saved, 0 not confirmed", StringComparison.Ordinal));
        Assert.IsTrue(page.Markup.Contains("audit log could not be confirmed", StringComparison.Ordinal));
        Assert.IsTrue(page.FindAll("button").Single(b => b.TextContent.Trim() == "Review import").HasAttribute("disabled"));
    }

    [TestMethod]
    public void UploadRejectsMalformedFileWithoutSaving()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Upload CSV");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("header\n1,2,3\n", "bad.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("must contain 14 columns", StringComparison.Ordinal)));
        Assert.AreEqual(0, fixture.Purchases.Added.Count);
        Assert.IsNull(fixture.Log.Value);
    }

    [TestMethod]
    public void UploadRejectsFilesOverFiveMegabytes()
    {
        using var fixture = new Fixture();
        var page = fixture.Context.RenderComponent<CreatePurchase>();
        Click(page, "Lines");
        Click(page, "Upload CSV");
        page.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(new byte[5 * 1024 * 1024 + 1], "large.csv"));
        page.WaitForAssertion(() => Assert.IsTrue(page.Markup.Contains("exceeds 5 MB", StringComparison.Ordinal)));
        Assert.AreEqual(0, fixture.Purchases.Added.Count);
        Assert.IsNull(fixture.Log.Value);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly BlendIdentityDbContext database;
        public Bunit.TestContext Context { get; } = new();
        public FakePurchases Purchases { get; } = new();
        public FakeDocuments Documents { get; } = new();
        public FakeLog Log { get; } = new();
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
            database.MenuItems.Add(new BlendMenuItem { Id = 1, CompanyId = company.Id, ModuleId = 1, Name = "Create Purchase", Path = "/Operations/Purchase" });
            database.MenuItemRoles.Add(new BlendMenuItemRole { MenuItemId = 1, RoleId = role.Id });
            database.MenuItemOperations.Add(new BlendMenuItemOperation { Id = 1, MenuItemId = 1, Key = "create", Name = "Create" });
            if (grant) database.RoleOperations.Add(new BlendRoleOperation { RoleId = role.Id, OperationId = 1 });
            database.SaveChanges();
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.JSInterop.SetupModule("./Components/Pages/Buyers.razor.js").Mode = JSRuntimeMode.Loose;
            Context.Services.AddSingleton(database);
            Context.Services.AddSingleton<IPurchaseService>(Purchases);
            Context.Services.AddSingleton<IDocumentService>(Documents);
            Context.Services.AddSingleton<IBrokerService>(new FakeBrokers());
            Context.Services.AddSingleton<IGardenMarkService>(new FakeMarks());
            Context.Services.AddSingleton<IGradeService>(new FakeGrades());
            Context.Services.AddSingleton<IPurchaseUploadLogService>(Log);
            Context.Services.AddSingleton<AuthenticationStateProvider>(new FixedAuth(TestData.Principal(company.Id, admin, role.Id)));
        }
        public void RevokeMenu() { database.MenuItemRoles.Remove(database.MenuItemRoles.Single()); database.SaveChanges(); }
        public void RevokeCreate() { database.RoleOperations.Remove(database.RoleOperations.Single()); database.SaveChanges(); }
        public void ChangeCompany() { database.Companies.Single().CompanyNumber = 3; database.SaveChanges(); }
        public void Dispose() { Context.Dispose(); database.Dispose(); }
    }

    private sealed class FixedAuth(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
    }
    private sealed class FakeDocuments : IDocumentService
    {
        public string Number { get; set; } = "0042P";
        public bool Fail { get; set; }
        public int Calls { get; private set; }
        public (int DocType, int CompanyId) LastRequest { get; private set; }
        public ManualResetEventSlim? ReadGate { get; set; }
        public DocumentNo GetDocumentNo(int docType, int companyId) { if (ReadGate is not null && !ReadGate.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Timed out waiting for the test to release the document read."); Calls++; LastRequest = (docType, companyId); if (Fail) throw new InvalidOperationException("Missing number"); return new() { FormattedDocNo = Number }; }
        public IReadOnlyList<DocumentNo> GetDocumentData() => throw new NotSupportedException();
        public IReadOnlyList<DocumentNo> GetDocumentDataByModule(int moduleId, int companyId) => throw new NotSupportedException();
        public BusinessOperationResult ResetDocumentNo(DocumentNo documentInfo) => throw new NotSupportedException();
        public bool IncrementDocumentNo(int docType) => throw new NotSupportedException();
    }
    private sealed class FakeBrokers : IBrokerService
    {
        public Broker? GetBrokerById(int brokerId) => throw new NotSupportedException();
        public IReadOnlyList<Broker> GetAllBrokers(string searchText, int companyId) => companyId == 2 ? [new() { BrokerId = 7, BrokerCode = "B", BrokerName = "Broker" }] : [];
        public BusinessOperationResult CreateBroker(Broker broker) => throw new NotSupportedException();
        public BusinessOperationResult UpdateBroker(Broker broker) => throw new NotSupportedException();
        public BusinessOperationResult DeleteBroker(int brokerId) => throw new NotSupportedException();
    }
    private sealed class FakeMarks : IGardenMarkService
    {
        public IReadOnlyList<GardenMark> GetGardenMarksList() => [];
        public BusinessOperationResult InsertGardenMark(string gardenMark) => throw new NotSupportedException();
    }
    private sealed class FakeGrades : IGradeService
    {
        public IReadOnlyList<Grade> GetGradeList() => [];
        public BusinessOperationResult InsertGrade(string grade) => throw new NotSupportedException();
    }
    private sealed class FakeLog : IPurchaseUploadLogService
    {
        public PurchaseUploadLog? Value { get; private set; }
        public bool Fail { get; set; }
        public void Insert(PurchaseUploadLog log) { if (Fail) throw new InvalidOperationException("Audit unavailable"); Value = log; }
    }
    private sealed class FakePurchases : IPurchaseService
    {
        public int LastCompany { get; private set; }
        public bool IsClosed { get; set; }
        public bool FailSecond { get; set; }
        public bool CloseAfterFirst { get; set; }
        public PurchaseSale? Created { get; private set; }
        public PurchaseSale? Closed { get; private set; }
        public StraightLine? UpdatedLine { get; private set; }
        public StraightLine? DeletedLine { get; private set; }
        public List<StraightLine> Added { get; } = [];
        public IReadOnlyList<StraightLine>? DisplayLines { get; set; }
        public ManualResetEventSlim? LineReadGate { get; set; }
        public IReadOnlyList<PurchaseSale> GetAllSales(string searchString, int companyId) { LastCompany = companyId; return [new() { SaleId = 17, CompanyId = companyId, SaleDate = "2026-11-18", IsClosed = IsClosed, SaleStatus = IsClosed ? "Closed" : "Open" }]; }
        public IReadOnlyList<StraightLine> GetAddedStraightLines(int saleId) { if (LineReadGate is not null && !LineReadGate.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Timed out waiting for the test to release the line read."); return DisplayLines ?? [new() { Id = 21, SaleId = saleId, BoxNo = "BOX", BrokerId = 7 }]; }
        public StraightLine? GetStraightLineByReservedNo(long id) => new() { Id = id, SaleId = 17, StockId = 31, RepeatingId = 22, RepeatingStockId = 32, RowVersion1 = [1], RowVersion2 = [2], BrokerId = 7, SaleType = "MS", SaleCode = "001", LotNo = "LOT", Invoice = "INV", GardenMark = "MARK", Grade = "BOP", Net1 = 5, Pack1 = 10, Net2 = 4, Pack2 = 2, ReducedQty = 1, Rate = 2, TeaCertificates = "Organic" };
        public IReadOnlyList<TeaCertificate> GetTeaCertificates() => [new() { CertifiedName = "Organic" }];
        public BusinessOperationResult CreateSale(PurchaseSale sale) { Created = sale; return BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateSale(PurchaseSale sale) => BusinessOperationResult.Success();
        public BusinessOperationResult CloseSale(PurchaseSale sale) { Closed = sale; return BusinessOperationResult.Success(); }
        public BusinessOperationResult OpenSale(PurchaseSale sale) => BusinessOperationResult.Success();
        public BusinessOperationResult AddStraightLine(StraightLine line) { Added.Add(line); if (CloseAfterFirst && Added.Count == 1) IsClosed = true; return FailSecond && Added.Count == 2 ? BusinessOperationResult.Failure("Rejected") : BusinessOperationResult.Success(); }
        public BusinessOperationResult UpdateStraightLine(StraightLine line) { UpdatedLine = line; return BusinessOperationResult.Success(); }
        public BusinessOperationResult DeleteStraightLine(StraightLine line) { DeletedLine = line; return BusinessOperationResult.Success(); }
    }
}
