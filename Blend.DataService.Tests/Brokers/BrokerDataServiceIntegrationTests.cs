using Blend.DataService.Brokers;
using Blend.DataService.Mapping;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.Brokers;

[TestClass]
public sealed class BrokerDataServiceIntegrationTests
{
    [TestMethod]
    public void GetBrokerById_LoadsBrokerFromConfiguredSqlServer()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        
        var brokerIdValue = Environment.GetEnvironmentVariable("BLEND_TEST_BROKER_ID");

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(brokerIdValue))
        {
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING and BLEND_TEST_BROKER_ID to run this live SQL Server test.");
        }

        if (!int.TryParse(brokerIdValue, out var brokerId) || brokerId <= 0)
        {
            Assert.Fail("BLEND_TEST_BROKER_ID must be a positive broker ID.");
        }

        var mapper = new DapperStoredProcedureMapper(() => new SqlConnection(connectionString));
        var service = new BrokerDataService(mapper);

        var broker = service.GetBrokerById(brokerId);

        Assert.IsNotNull(broker, $"Stored procedure trn_BrokerGetById returned no broker for ID {brokerId}.");
        Assert.AreEqual(brokerId, broker.BrokerId);
    }
}
