using Blend.BlazorApp.Authentication.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class PermissionGrantMigrationTests
{
    [TestMethod]
    public void MigrationCopiesBrokerRoleAndOperationGrantsToBothPages()
    {
        var migration = new GrantDocumentNumbersAndTeaStandardsPermissions();
        var attribute = typeof(GrantDocumentNumbersAndTeaStandardsPermissions).GetCustomAttributes(typeof(MigrationAttribute), false).Cast<MigrationAttribute>().Single();
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql;

        Assert.AreEqual("20261117235000_GrantDocumentNumbersAndTeaStandardsPermissions", attribute.Id);
        foreach (var expected in new[] { "N'/DocumentNumbers'", "N'/TeaStandards'", "N'/Brokers'", "INSERT INTO [MenuItemRoles]", "INSERT INTO [RoleOperations]" })
        {
            Assert.IsTrue(sql.Contains(expected, StringComparison.Ordinal), expected);
        }
    }
}
