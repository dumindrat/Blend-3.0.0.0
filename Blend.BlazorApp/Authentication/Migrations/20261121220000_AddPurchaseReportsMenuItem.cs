using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations;

[DbContext(typeof(BlendIdentityDbContext))]
[Migration("20261121220000_AddPurchaseReportsMenuItem")]
public partial class AddPurchaseReportsMenuItem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO [MenuItems] ([CompanyId], [ModuleId], [Name], [GroupName], [Path], [SortOrder])
            SELECT company.[Id], COALESCE(purchase.[ModuleId], blend.[Id], fallback.[Id]),
                N'Purchase Reports', N'Report', N'/Reports/Purchase', 50
            FROM [Companies] AS company
            OUTER APPLY (SELECT TOP (1) [ModuleId] FROM [MenuItems] WHERE [CompanyId] = company.[Id] AND [Path] = N'/Operations/Purchase' ORDER BY [Id]) AS purchase
            OUTER APPLY (SELECT TOP (1) [Id] FROM [Modules] WHERE [CompanyId] = company.[Id] AND [Name] = N'Blend' ORDER BY [Id]) AS blend
            OUTER APPLY (SELECT TOP (1) [Id] FROM [Modules] WHERE [CompanyId] = company.[Id] ORDER BY [Id]) AS fallback
            WHERE COALESCE(purchase.[ModuleId], blend.[Id], fallback.[Id]) IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM [MenuItems] WHERE [CompanyId] = company.[Id] AND [Path] = N'/Reports/Purchase');

            INSERT INTO [MenuItemOperations] ([MenuItemId], [Key], [Name], [SortOrder])
            SELECT item.[Id], operation.[Key], operation.[Name], operation.[SortOrder]
            FROM [MenuItems] AS item
            CROSS JOIN (VALUES (N'create', N'Create', 0), (N'edit', N'Edit', 1), (N'delete', N'Delete', 2), (N'print', N'Print', 3)) AS operation([Key], [Name], [SortOrder])
            WHERE item.[Path] = N'/Reports/Purchase'
                AND NOT EXISTS (SELECT 1 FROM [MenuItemOperations] AS existing WHERE existing.[MenuItemId] = item.[Id] AND existing.[Key] = operation.[Key]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve menu items and permissions that administrators may have configured.
    }
}
