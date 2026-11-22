using System.Data;
using Blend.BlazorApp.Services;
using Blend.BusinessService.Reports.Purchase;
using Blend.DataService.Mapping;
using Blend.DataService.Reports.Purchase;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class SalesSummaryReportServiceIntegrationTests
{
    [TestMethod]
    public void ReadsOnlyTheRequestedCompanySaleUsingTheSummaryProcedure()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");

        using var connection = new SqlConnection(connectionString);
        var sale = connection.QueryFirstOrDefault<(int Id, int CompanyId)>(
            "SELECT TOP (1) h.Id, h.CompanyId FROM purchase_header h WHERE h.CompanyId > 0 AND EXISTS (SELECT 1 FROM purchase_detail p WHERE p.SaleId = h.Id) ORDER BY h.Id");
        if (sale.Id == 0) Assert.Inconclusive("The test database contains no company sale with purchase details.");

        var environment = new TestHostEnvironment();
        var mapper = new DapperStoredProcedureMapper(() => new SqlConnection(connectionString));
        var reports = new PurchaseReportService(new PurchaseReportDataService(mapper));
        var service = new SalesSummaryReportService(reports, new SalesSummaryPdfService(environment), environment,
            () => new SqlConnection(connectionString));
        Assert.ThrowsExactly<SaleNotInCompanyException>(() => service.Generate(sale.Id, "Integration Test Company", sale.CompanyId == 1 ? 2 : 1));
        var expected = connection.Query<SaleSummaryRecord>("rpt_SalesSummary", new { saleno = sale.Id }, commandType: CommandType.StoredProcedure).ToArray();
        if (expected.Length == 0) Assert.Inconclusive("The selected sale has no summary rows.");
        var actual = service.Generate(sale.Id, "Integration Test Company", sale.CompanyId);
        using var document = UglyToad.PdfPig.PdfDocument.Open(actual);
        var text = string.Join("\n", document.GetPages().Select(page => UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor.ContentOrderTextExtractor.GetText(page)));
        StringAssert.Contains(text, "Integration Test Company");
        StringAssert.Contains(text, expected[0].SaleDate!);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
