using Blend.DataService.Mapping;
using Blend.DataService.Purchases;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.Purchases;

[TestClass]
public sealed class PurchaseDataServiceIntegrationTests
{
    [TestMethod]
    public void ReadProceduresMapSalesLinesAndCertificates()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");
        var service = new PurchaseDataService(new DapperStoredProcedureMapper(() => new SqlConnection(connectionString)), () => new SqlConnection(connectionString));
        var sales = service.GetSales(string.Empty, 1);
        service.GetTeaCertificates();
        Assert.IsTrue(sales.All(sale => sale.Id > 0));
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP 1 p.SaleId FROM purchase_detail p JOIN purchase_header h ON h.Id = p.SaleId JOIN teastock s ON s.StockItemId = p.Id AND s.ItemType = 'L' JOIN broker b ON b.BrokerId = p.BrokerId WHERE h.CompanyId = 1 AND p.IsVoid = 0 ORDER BY p.Id DESC";
        var result = command.ExecuteScalar();
        if (result is null) Assert.Inconclusive("The database has no company 1 purchase lines to inspect.");
        var saleId = Convert.ToInt32(result);
        var lines = service.GetLinesBySale(saleId);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines.All(line => line.Id > 0 && line.StockId > 0 && line.SaleId == saleId));
        Assert.IsTrue(service.GetLinesByReservedNo(lines[0].Id).Any(line => line.Id == lines[0].Id));
    }
}
