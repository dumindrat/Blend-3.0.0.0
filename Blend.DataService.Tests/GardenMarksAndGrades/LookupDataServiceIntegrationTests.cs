using Blend.DataService.GardenMarks;
using Blend.DataService.Grades;
using Blend.DataService.Mapping;
using Microsoft.Data.SqlClient;

namespace Blend.DataService.Tests.GardenMarksAndGrades;

[TestClass]
public sealed class LookupDataServiceIntegrationTests
{
    [TestMethod]
    public void ReadProceduresMapConfiguredDatabaseResults()
    {
        var connectionString = Environment.GetEnvironmentVariable("BLEND_TEST_SQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Inconclusive("Set BLEND_TEST_SQL_CONNECTION_STRING to run this read-only SQL Server test.");
        }

        var mapper = new DapperStoredProcedureMapper(() => new SqlConnection(connectionString));
        var gardens = new GardenMarkDataService(mapper).GetGardenMarksList();
        var grades = new GradeDataService(mapper).GetGradeList();
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GardenMarkName FROM dbo.GardenMark ORDER BY Id; SELECT GradeName FROM dbo.Grade ORDER BY GradeName;";
        using var reader = command.ExecuteReader();
        var expectedGardens = new List<string?>();
        while (reader.Read()) expectedGardens.Add(reader.IsDBNull(0) ? null : reader.GetString(0));
        Assert.IsTrue(reader.NextResult());
        var expectedGrades = new List<string?>();
        while (reader.Read()) expectedGrades.Add(reader.IsDBNull(0) ? null : reader.GetString(0));
        CollectionAssert.AreEqual(expectedGardens, gardens.Select(item => item.GardenMarkName).ToList());
        CollectionAssert.AreEqual(expectedGrades, grades.Select(item => item.GradeName).ToList());
    }
}
