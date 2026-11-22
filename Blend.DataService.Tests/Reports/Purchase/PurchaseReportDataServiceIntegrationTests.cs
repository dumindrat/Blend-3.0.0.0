using Blend.DataService.Mapping;
using Blend.DataService.Reports.Purchase;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.Reports.Purchase;

[TestClass]
public sealed class PurchaseReportDataServiceIntegrationTests
{
    [TestMethod]
    public void ReportProceduresAcceptLegacyParameters()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");

        var service = new PurchaseReportDataService(new DapperStoredProcedureMapper(() => new SqlConnection(connectionString)));
        service.GetSaleSummary(0);
        service.GetPurchaseSheet(0);
        service.GetCanisterSheet(0);
        service.GetPurchaseSheetByAdvanceFilter("1900-01-01", "1900-01-01", "", "", "", "");
        service.GetPurchaseSheetByBroker("1900-01-01", "1900-01-01", 0);
    }
}
