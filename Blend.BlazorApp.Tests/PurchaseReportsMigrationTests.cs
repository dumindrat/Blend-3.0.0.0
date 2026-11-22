using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Authentication.Migrations;
using Blend.BlazorApp.Components.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class PurchaseReportsMigrationTests
{
    [TestMethod]
    public void MigrationAddsCompanyScopedReportMenuWithoutGrantingRoles()
    {
        var migration = new AddPurchaseReportsMenuItem();
        var attribute = typeof(AddPurchaseReportsMenuItem).GetCustomAttributes(typeof(MigrationAttribute), false).Cast<MigrationAttribute>().Single();
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql;
        Assert.AreEqual("20261121220000_AddPurchaseReportsMenuItem", attribute.Id);
        foreach (var expected in new[] { "[Companies]", "[Modules]", "[CompanyId]", "N'/Operations/Purchase'", "N'Purchase Reports'", "N'Report'", "N'/Reports/Purchase'", "[MenuItemOperations]", "N'print'", "NOT EXISTS" })
            Assert.IsTrue(sql.Contains(expected, StringComparison.Ordinal), expected);
        Assert.IsFalse(sql.Contains("INSERT INTO [MenuItemRoles]", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(sql.Contains("INSERT INTO [RoleOperations]", StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual(0, migration.DownOperations.Count);
    }

    [TestMethod]
    public void SqlServerDiscoversMigrationAndGeneratesScript()
    {
        var options = new DbContextOptionsBuilder<BlendIdentityDbContext>()
            .UseSqlServer("Server=localhost;Database=Unused;Integrated Security=true;TrustServerCertificate=true")
            .Options;
        using var database = new BlendIdentityDbContext(options);
        var migrations = database.Database.GetMigrations().ToList();
        var index = migrations.IndexOf("20261121220000_AddPurchaseReportsMenuItem");
        Assert.IsTrue(index > 0);
        var script = database.GetService<IMigrator>().GenerateScript(migrations[index - 1], migrations[index]);
        StringAssert.Contains(script, "N'/Reports/Purchase'");
        StringAssert.Contains(script, "20261121220000_AddPurchaseReportsMenuItem");
    }

    [TestMethod]
    public void RouteMatchesMenuPathAndRequiresAuthorization()
    {
        Assert.IsTrue(typeof(PurchaseReports).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Any(route => route.Template == "/Reports/Purchase"));
        Assert.IsTrue(typeof(PurchaseReports).GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Any(attribute => attribute.Policy == "PasswordChangeComplete"));
    }
}
