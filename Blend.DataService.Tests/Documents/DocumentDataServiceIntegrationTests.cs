using Blend.DataService.Documents;
using Blend.DataService.Mapping;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.Documents;

[TestClass]
public sealed class DocumentDataServiceIntegrationTests
{
    [TestMethod]
    public void ReadProcedures_MapConfiguredDatabaseResults()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");
        }

        var service = new DocumentDataService(new DapperStoredProcedureMapper(() => new SqlConnection(connectionString)));
        var documents = service.GetDocuments();
        var document = documents.FirstOrDefault(item => item.CompanyId.HasValue);
        if (document is null)
        {
            Assert.Inconclusive("The database contains no document with a company ID to verify the read procedures.");
        }

        var numbers = service.GetDocumentNumbers(document.DocType);

        Assert.IsTrue(numbers.Any(item => item.DocType == document.DocType && item.CompanyId == document.CompanyId),
            "The document-number procedure did not return the document's type and company.");
    }
}
