using System.IO.Compression;
using System.Xml.Linq;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Services;
using Blend.BusinessService.Reports.Purchase;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class SalesSummaryReportServiceTests
{
    [TestMethod]
    public void GeneratesPdfFromRequestedSaleAndCompany()
    {
        var purchases = new FakePurchaseReports
        {
            Rows = [
                new() { SaleDate = "2031-02-14", BrokerId = 19, BrokerName = "Example Test Broker", SaleType = "MS", NoOfLots = 4, Weight = 100, Value = 2500, NinetyPercVal = 2250, TenPercVal = 250, HundredPercVal = 2500 }
            ]
        };
        var connection = new SalesSummaryTestConnection();
        purchases.OnQuery = () => { Assert.IsNotNull(connection.Transaction); Assert.IsFalse(connection.Transaction.Disposed); Assert.IsFalse(connection.Transaction.Committed); };
        var service = CreateService(purchases, Environments.Development, connection);
        var before = DateTimeOffset.UtcNow;
        using var document = PdfDocument.Open(service.Generate(123, "Test Company", 2, "report-user", SalesSummaryOutput.Pdf, CompanyDateFormats.DayFirst));
        var after = DateTimeOffset.UtcNow;
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        StringAssert.Contains(text, "Printed by: report-user");
        Assert.IsTrue(Enumerable.Range(0, (int)(after - before).TotalSeconds + 2).Any(i => text.Contains(before.AddSeconds(i).ToString("yyyy-MM-dd HH:mm:ss") + " UTC", StringComparison.Ordinal)));
        Assert.AreEqual(123, purchases.SaleId);
        Assert.AreEqual(123, connection.Arguments["SaleId"]);
        Assert.AreEqual(2, connection.Arguments["CompanyId"]);
        Assert.AreEqual(System.Data.IsolationLevel.Serializable, connection.Transaction!.IsolationLevel);
        Assert.IsTrue(connection.Transaction.Committed);
        foreach (var expected in new[] { "Test Company", "14/02/2031", "Example Test Broker", "2,500.00", "25.00" })
            StringAssert.Contains(text, expected);
        Assert.IsFalse(text.Contains("SAMPLE - NOT REAL DATA", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GeneratesExcelWorkbookWithSummaryAndNumericCells()
    {
        var purchases = new FakePurchaseReports
        {
            Rows = [new() { SaleDate = "2031-02-14", BrokerId = 19, BrokerName = "Example Test Broker", SaleType = "MS", NoOfLots = 4, Weight = 100, Value = 2500, NinetyPercVal = 2250, TenPercVal = 250, HundredPercVal = 2500 }]
        };
        var service = CreateService(purchases, Environments.Production);
        var bytes = service.Generate(123, "Test Company", 2, "report-user", SalesSummaryOutput.Excel, CompanyDateFormats.DayFirst);
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        foreach (var entry in archive.Entries.Where(entry => entry.FullName.EndsWith(".xml", StringComparison.Ordinal) || entry.FullName.EndsWith(".rels", StringComparison.Ordinal)))
        {
            using var entryStream = entry.Open();
            _ = XDocument.Load(entryStream);
        }
        var workbookEntry = archive.GetEntry("xl/workbook.xml");
        Assert.IsNotNull(workbookEntry);
        using var workbookStream = workbookEntry.Open();
        var workbook = XDocument.Load(workbookStream);
        XNamespace workbookNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.AreEqual("Sales Summary", (string?)workbook.Descendants(workbookNs + "sheet").Single().Attribute("name"));
        var worksheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
        Assert.IsNotNull(worksheetEntry);
        using var worksheetStream = worksheetEntry.Open();
        var worksheet = XDocument.Load(worksheetStream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var text = worksheet.Descendants(ns + "t").Select(element => element.Value).ToArray();
        CollectionAssert.Contains(text, "14/02/2031");
        foreach (var value in new[] { "Broker", "Type", "Lots", "Weight", "Value", "90%", "10%", "Example Test Broker", "No Of Lots:", "Total Sale Qty:", "Total Value:", "Total 10% of Adv:", "Total 90% Bal:", "Sale Avg:", "Printed by", "report-user" })
            CollectionAssert.Contains(text, value);
        CollectionAssert.DoesNotContain(text, "BrokerId");
        CollectionAssert.DoesNotContain(text, "100%");
        var cells = worksheet.Descendants(ns + "c").ToDictionary(cell => (string)cell.Attribute("r")!, cell => cell);
        foreach (var (reference, expected) in new[] { ("C6", "4"), ("D6", "100"), ("E6", "2500"), ("F6", "2250"), ("G6", "250"), ("B9", "4"), ("B10", "100"), ("B11", "2500"), ("B12", "250"), ("B13", "2250"), ("B14", "25") })
            Assert.AreEqual(expected, (string?)cells[reference].Element(ns + "v"), reference);
        var stylesEntry = archive.GetEntry("xl/styles.xml");
        Assert.IsNotNull(stylesEntry);
        using var stylesStream = stylesEntry.Open();
        var styles = XDocument.Load(stylesStream);
        Assert.AreEqual("none", (string?)styles.Descendants(ns + "patternFill").First().Attribute("patternType"));
        Assert.AreEqual(123, purchases.SaleId);
    }

    [TestMethod]
    public void RejectsEmptyResultsAndPropagatesQueryFailures()
    {
        var purchases = new FakePurchaseReports();
        var service = CreateService(purchases, Environments.Development);
        var noData = Assert.ThrowsExactly<EmptySalesSummaryException>(() => service.Generate(7, "Company", 2, "user"));
        StringAssert.Contains(noData.Message, "No sales summary data");
        Assert.AreEqual(7, purchases.SaleId);
        var failure = new InvalidOperationException("Query failed");
        purchases.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.Generate(8, "Company", 2, "user")));
    }

    [TestMethod]
    public void RejectsInvalidSaleAndProductionBeforeQuery()
    {
        var purchases = new FakePurchaseReports();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateService(purchases, Environments.Development).Generate(0, "Company", 2, "user"));
        Assert.ThrowsExactly<InvalidOperationException>(() => CreateService(purchases, Environments.Production).Generate(123, "Company", 2, "user"));
        Assert.IsNull(purchases.SaleId);
    }

    [TestMethod]
    public void GenerateRejectsInvalidIdentifiersAndWrongCompanyBeforeQuery()
    {
        var purchases = new FakePurchaseReports();
        var connection = new SalesSummaryTestConnection { OwnsSale = false };
        var service = CreateService(purchases, Environments.Development, connection);
        var invalidSale = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.Generate(0, "Company", 1, "user"));
        var invalidCompany = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.Generate(1, "Company", 0, "user"));
        Assert.ThrowsExactly<ArgumentException>(() => service.Generate(1, "Company", 2, " "));
        Assert.AreEqual("saleId", invalidSale.ParamName);
        Assert.AreEqual("companyId", invalidCompany.ParamName);
        Assert.IsNull(connection.Transaction);
        Assert.ThrowsExactly<SaleNotInCompanyException>(() => service.Generate(123, "Company", 2, "user"));
        Assert.IsNull(purchases.SaleId);
        Assert.IsFalse(connection.Transaction!.Committed);
    }

    private static SalesSummaryReportService CreateService(FakePurchaseReports purchases, string environmentName, SalesSummaryTestConnection? connection = null)
    {
        var environment = new TestEnvironment(environmentName);
        return new SalesSummaryReportService(purchases, new SalesSummaryPdfService(environment), environment,
            () => connection ?? new SalesSummaryTestConnection());
    }

    private sealed class FakePurchaseReports : IPurchaseReportService
    {
        public int? SaleId { get; private set; }
        public IReadOnlyList<SaleSummary> Rows { get; set; } = [];
        public Exception? Failure { get; set; }
        public Action? OnQuery { get; set; }
        public IReadOnlyList<SaleSummary> GetSaleSummary(int saleId)
        {
            OnQuery?.Invoke();
            SaleId = saleId;
            if (Failure is not null) throw Failure;
            return Rows;
        }
        public IReadOnlyList<PurchaseSheet> GetPurchaseSheet(int saleId) => throw new NotSupportedException();
        public IReadOnlyList<CanisterSheet> GetCanisterSheet(int saleId) => throw new NotSupportedException();
        public IReadOnlyList<PurchaseSheet> GetPurchaseSheetByAdvanceFilter(PurchaseSheetFilter filter) => throw new NotSupportedException();
        public IReadOnlyList<PurchaseSheet> GetPurchaseSheetByBroker(BrokerPurchaseFilter filter) => throw new NotSupportedException();
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
