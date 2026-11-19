using Blend.BlazorApp.Authentication.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class BuyerContractsMigrationTests
{
    [TestMethod]
    public void MigrationCreatesCompanyMenuAndCopiesBrokerPermissions()
    {
        var migration = new AddBuyerContractsMenuItem();
        var attribute = typeof(AddBuyerContractsMenuItem).GetCustomAttributes(typeof(MigrationAttribute), false).Cast<MigrationAttribute>().Single();
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql;
        Assert.AreEqual("20261118220000_AddBuyerContractsMenuItem", attribute.Id);
        foreach (var expected in new[] { "N'/BuyerContracts'", "N'Buyer Contracts'", "[Companies]", "[MenuItemOperations]", "[MenuItemRoles]", "[RoleOperations]", "N'/Brokers'", "N'create'", "N'edit'", "N'delete'" })
            Assert.IsTrue(sql.Contains(expected, StringComparison.Ordinal), expected);
    }
}
