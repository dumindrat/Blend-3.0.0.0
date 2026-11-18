using Blend.BlazorApp.Authentication.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class DocumentNumbersMigrationTests
{
    [TestMethod]
    public void MigrationSeedsCompanyMenuAndOperations()
    {
        var migration = new AddDocumentNumbersMenuItem();
        var attribute = typeof(AddDocumentNumbersMenuItem).GetCustomAttributes(typeof(MigrationAttribute), false).Cast<MigrationAttribute>().Single();
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql;

        Assert.AreEqual("20261117230000_AddDocumentNumbersMenuItem", attribute.Id);
        Assert.IsTrue(sql.Contains("N'Document Numbers'", StringComparison.Ordinal));
        Assert.IsTrue(sql.Contains("N'/DocumentNumbers'", StringComparison.Ordinal));
        Assert.IsTrue(sql.Contains("[Companies]", StringComparison.Ordinal));
        Assert.IsTrue(sql.Contains("[MenuItemOperations]", StringComparison.Ordinal));
        Assert.IsTrue(sql.Contains("N'edit'", StringComparison.Ordinal));
        Assert.IsTrue(sql.Contains("NOT EXISTS", StringComparison.Ordinal));
    }
}
