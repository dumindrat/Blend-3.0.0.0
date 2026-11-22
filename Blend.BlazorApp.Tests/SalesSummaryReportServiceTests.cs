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
        using var document = PdfDocument.Open(service.Generate(123, "Test Company", 2));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        Assert.AreEqual(123, purchases.SaleId);
        Assert.AreEqual(123, connection.Arguments["SaleId"]);
        Assert.AreEqual(2, connection.Arguments["CompanyId"]);
        Assert.AreEqual(System.Data.IsolationLevel.Serializable, connection.Transaction!.IsolationLevel);
        Assert.IsTrue(connection.Transaction.Committed);
        foreach (var expected in new[] { "Test Company", "2031-02-14", "Example Test Broker", "2,500.00", "25.00" })
            StringAssert.Contains(text, expected);
        Assert.IsFalse(text.Contains("SAMPLE - NOT REAL DATA", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RejectsEmptyResultsAndPropagatesQueryFailures()
    {
        var purchases = new FakePurchaseReports();
        var service = CreateService(purchases, Environments.Development);
        var noData = Assert.ThrowsExactly<EmptySalesSummaryException>(() => service.Generate(7, "Company", 2));
        StringAssert.Contains(noData.Message, "No sales summary data");
        Assert.AreEqual(7, purchases.SaleId);
        var failure = new InvalidOperationException("Query failed");
        purchases.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.Generate(8, "Company", 2)));
    }

    [TestMethod]
    public void RejectsInvalidSaleAndProductionBeforeQuery()
    {
        var purchases = new FakePurchaseReports();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateService(purchases, Environments.Development).Generate(0, "Company", 2));
        Assert.ThrowsExactly<InvalidOperationException>(() => CreateService(purchases, Environments.Production).Generate(123, "Company", 2));
        Assert.IsNull(purchases.SaleId);
    }

    [TestMethod]
    public void GenerateRejectsInvalidIdentifiersAndWrongCompanyBeforeQuery()
    {
        var purchases = new FakePurchaseReports();
        var connection = new SalesSummaryTestConnection { OwnsSale = false };
        var service = CreateService(purchases, Environments.Development, connection);
        var invalidSale = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.Generate(0, "Company", 1));
        var invalidCompany = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.Generate(1, "Company", 0));
        Assert.AreEqual("saleId", invalidSale.ParamName);
        Assert.AreEqual("companyId", invalidCompany.ParamName);
        Assert.IsNull(connection.Transaction);
        Assert.ThrowsExactly<SaleNotInCompanyException>(() => service.Generate(123, "Company", 2));
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
