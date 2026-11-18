using Blend.DataService.Contracts;
using Blend.DataService.Mapping;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.Contracts;

[TestClass]
public sealed class ContractDataServiceIntegrationTests
{
    [TestMethod]
    public void ReadProceduresAcceptCompanyAndMapResults()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");
        }

        var service = new ContractDataService(new DapperStoredProcedureMapper(() => new SqlConnection(connectionString)));
        var contracts = service.GetContracts("", 1);
        var open = service.GetOpenContracts(1);
        var contract = contracts.FirstOrDefault();
        if (contract is not null)
        {
            service.GetContractDetails(contract.ContractId);
            service.SearchAvailableStandards(contract.ContractId, "", 1);
        }
        Assert.IsTrue(open.All(item => !item.IsClosed));
    }
}
