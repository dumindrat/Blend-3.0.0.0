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
    public void GeneratesPurchaseSheetExcelAndPdfForRequestedSale()
    {
        var purchases = new FakePurchaseReports
        {
            SheetRows = [
                new() { BoxNo = "B-01", TinNo = "T-02", LotNo = "L-03", Invoice = "INV-4", Grade = "BOP", Standard = "S1", GardenMark = "Garden", SaleDate = "2031-02-14", SaleType = "MS", SaleCode = "M", BrokerName = "Example Broker", NetWeight = 8, Packs = 2, Weight = 16, Rate = 4.5m, Value = 72, IsRepeated = true, VirtualValue = 7, VirtualWeight = 3, TeaCertificates = "ORG" },
                new() { BoxNo = "B-02", LotNo = "L-04", Invoice = "INV-5", Grade = "BOPF", Standard = "S2", GardenMark = "Garden", SaleDate = "2031-02-14", SaleType = "MS", SaleCode = "M", BrokerName = "Example Broker", NetWeight = 6, Packs = 3, Rate = 10, Value = 30, VirtualValue = 3, VirtualWeight = 4 }
            ]
        };
        var productionService = CreatePurchaseSheetService(purchases, Environments.Production);
        var bytes = productionService.Generate(123, "Test Company", 2, "report-user", PurchaseSheetReportOutput.Excel, CompanyDateFormats.DayFirst, withValues: true);
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        foreach (var packageEntry in archive.Entries.Where(item => item.FullName.EndsWith(".xml", StringComparison.Ordinal) || item.FullName.EndsWith(".rels", StringComparison.Ordinal)))
        {
            using var packageStream = packageEntry.Open();
            _ = XDocument.Load(packageStream);
        }
        var workbookEntry = archive.GetEntry("xl/workbook.xml");
        Assert.IsNotNull(workbookEntry);
        using var workbookStream = workbookEntry.Open();
        var workbook = XDocument.Load(workbookStream);
        XNamespace workbookNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.AreEqual("Purchase Sheet", (string?)workbook.Descendants(workbookNs + "sheet").Single().Attribute("name"));
        var entry = archive.GetEntry("xl/worksheets/sheet1.xml");
        Assert.IsNotNull(entry);
        using var stream = entry.Open();
        var worksheet = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var values = worksheet.Descendants(ns + "t").Select(element => element.Value).ToArray();
        foreach (var expected in new[] { "Purchase Sheet - Test Company", "Sale date", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade", "Packs", "Net Weight", "Weight", "Rate", "Value", "B-01", "Example Broker", "14/02/2031", "Printed by", "report-user" })
            CollectionAssert.Contains(values, expected);
        foreach (var removedHeader in new[] { "Tin", "Virtual Value", "Virtual Weight", "Repeated", "Tea Certificates" })
            CollectionAssert.DoesNotContain(values, removedHeader);
        var headerOrder = worksheet.Descendants(ns + "row").Single(row => (string?)row.Attribute("r") == "4")
            .Elements(ns + "c").Select(cell => cell.Descendants(ns + "t").Single().Value).ToArray();
        CollectionAssert.AreEqual(new[] { "No", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade", "Packs", "Net Weight", "Weight", "Rate", "Value" }, headerOrder);
        var cells = worksheet.Descendants(ns + "c").ToDictionary(cell => (string)cell.Attribute("r")!, cell => cell);
        Assert.AreEqual("14/02/2031", cells["B2"].Descendants(ns + "t").Single().Value);
        Assert.AreEqual("1", cells["A5"].Descendants(ns + "t").Single().Value);
        Assert.AreEqual("2", cells["A6"].Descendants(ns + "t").Single().Value);
        Assert.AreEqual("2", (string?)cells["L5"].Element(ns + "v"));
        Assert.AreEqual("8", (string?)cells["M5"].Element(ns + "v"));
        Assert.AreEqual("3", (string?)cells["N5"].Element(ns + "v"));
        Assert.AreEqual("4.5", (string?)cells["O5"].Element(ns + "v"));
        Assert.AreEqual("7", (string?)cells["P5"].Element(ns + "v"));
        Assert.AreEqual("7", (string?)cells["N7"].Element(ns + "v"));
        Assert.AreEqual("10", (string?)cells["P7"].Element(ns + "v"));
        Assert.AreEqual("Total", cells["F7"].Descendants(ns + "t").Single().Value);
        CollectionAssert.Contains(values, "No of Lots :");
        CollectionAssert.Contains(values, "Tot No of Chests :");
        CollectionAssert.Contains(values, "Total Purchase Qty :");
        CollectionAssert.Contains(values, "Total Purchase Value :");
        Assert.AreEqual("2", (string?)cells["B8"].Element(ns + "v"));
        Assert.AreEqual("5", (string?)cells["B9"].Element(ns + "v"));
        Assert.AreEqual("7", (string?)cells["B10"].Element(ns + "v"));
        Assert.AreEqual("10", (string?)cells["B11"].Element(ns + "v"));
        var columnWidths = worksheet.Descendants(ns + "col").Select(column =>
            ((string?)column.Attribute("min"), (string?)column.Attribute("max"), (string?)column.Attribute("width"))).ToArray();
        CollectionAssert.Contains(columnWidths, ("1", "1", "5"));
        CollectionAssert.Contains(columnWidths, ("2", "3", "9"));
        CollectionAssert.Contains(columnWidths, ("4", "6", "4"));
        CollectionAssert.Contains(columnWidths, ("7", "7", "9"));
        CollectionAssert.Contains(columnWidths, ("8", "8", "11"));
        CollectionAssert.Contains(columnWidths, ("9", "9", "20"));
        CollectionAssert.Contains(columnWidths, ("12", "12", "6"));
        CollectionAssert.Contains(columnWidths, ("13", "13", "9"));
        Assert.AreEqual(123, purchases.SaleId);

        var pdfService = CreatePurchaseSheetService(purchases, Environments.Development);
        using var document = PdfDocument.Open(pdfService.Generate(123, "Test Company", 2, "report-user", PurchaseSheetReportOutput.Pdf, CompanyDateFormats.DayFirst, withValues: false));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        foreach (var expected in new[] { "Purchase Sheet - Test Company", "Sale date: 14/02/2031", "No", "B-01", "INV-4", "Example", "Broker", "Printed by: report-user" })
            StringAssert.Contains(text, expected);
        Assert.IsFalse(text.Contains("72.00", StringComparison.Ordinal));

        using var valuedDocument = PdfDocument.Open(pdfService.Generate(123, "Test Company", 2, "report-user", PurchaseSheetReportOutput.Pdf, CompanyDateFormats.DayFirst, withValues: true));
        var valuedText = string.Join("\n", valuedDocument.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        var titlePosition = valuedText.IndexOf("Purchase Sheet - Test Company", StringComparison.Ordinal);
        var datePosition = valuedText.IndexOf("Sale date: 14/02/2031", StringComparison.Ordinal);
        var lotsPosition = valuedText.IndexOf("No of Lots", StringComparison.Ordinal);
        var chestsPosition = valuedText.IndexOf("Tot No of Chests", StringComparison.Ordinal);
        var quantityPosition = valuedText.IndexOf("Total Purchase Qty", StringComparison.Ordinal);
        var valuePosition = valuedText.IndexOf("Total Purchase Value", StringComparison.Ordinal);
        Assert.IsTrue(titlePosition >= 0 && titlePosition < datePosition && datePosition < lotsPosition && lotsPosition < chestsPosition && chestsPosition < quantityPosition && quantityPosition < valuePosition);
        StringAssert.Contains(valuedText, "2");
        StringAssert.Contains(valuedText, "5.00");
        StringAssert.Contains(valuedText, "7.00");
        StringAssert.Contains(valuedText, "10.00");
    }

    [TestMethod]
    public void RejectsEmptyPurchaseSheetResults()
    {
        var purchases = new FakePurchaseReports();
        var service = CreatePurchaseSheetService(purchases, Environments.Production);
        var exception = Assert.ThrowsExactly<EmptyPurchaseSheetException>(() => service.Generate(7, "Company", 2, "user", PurchaseSheetReportOutput.Excel, CompanyDateFormats.MonthFirst, withValues: true));
        StringAssert.Contains(exception.Message, "No purchase sheet data");
        Assert.AreEqual(7, purchases.SaleId);
    }

    [TestMethod]
    public void PurchaseSheetServiceChecksSaleOwnershipAndCommitsSuccessfulGeneration()
    {
        var purchases = new FakePurchaseReports
        {
            SheetRows = [new() { BoxNo = "B-01", SaleDate = "2031-02-14" }]
        };
        var connection = new SalesSummaryTestConnection();
        var service = CreatePurchaseSheetService(purchases, Environments.Production, connection);

        var result = service.Generate(123, "Company", 2, "user", PurchaseSheetReportOutput.Excel, CompanyDateFormats.MonthFirst, withValues: false);

        Assert.IsTrue(result.Length > 0);
        Assert.AreEqual(123, connection.Arguments["SaleId"]);
        Assert.AreEqual(2, connection.Arguments["CompanyId"]);
        Assert.AreEqual(123, purchases.SaleId);
        Assert.IsTrue(connection.Transaction!.Committed);
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

    private static PurchaseSheetReportService CreatePurchaseSheetService(FakePurchaseReports purchases, string environmentName, SalesSummaryTestConnection? connection = null)
    {
        var environment = new TestEnvironment(environmentName);
        return new PurchaseSheetReportService(purchases, new PurchaseSheetPdfService(environment), new PurchaseSheetExcelService(), environment,
            () => connection ?? new SalesSummaryTestConnection());
    }

    private sealed class FakePurchaseReports : IPurchaseReportService
    {
        public int? SaleId { get; private set; }
        public IReadOnlyList<SaleSummary> Rows { get; set; } = [];
        public IReadOnlyList<PurchaseSheet> SheetRows { get; set; } = [];
        public Exception? Failure { get; set; }
        public Action? OnQuery { get; set; }
        public IReadOnlyList<SaleSummary> GetSaleSummary(int saleId)
        {
            OnQuery?.Invoke();
            SaleId = saleId;
            if (Failure is not null) throw Failure;
            return Rows;
        }
        public IReadOnlyList<PurchaseSheet> GetPurchaseSheet(int saleId) { SaleId = saleId; if (Failure is not null) throw Failure; return SheetRows; }
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
