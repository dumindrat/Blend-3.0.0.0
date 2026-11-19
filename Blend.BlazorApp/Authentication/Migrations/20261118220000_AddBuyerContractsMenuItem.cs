using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations;

[DbContext(typeof(BlendIdentityDbContext))]
[Migration("20261118220000_AddBuyerContractsMenuItem")]
public partial class AddBuyerContractsMenuItem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO [MenuItems] ([CompanyId], [ModuleId], [Name], [GroupName], [Path], [SortOrder])
            SELECT company.[Id], COALESCE(broker.[ModuleId], (SELECT MIN(module.[Id]) FROM [Modules] module WHERE module.[CompanyId] = company.[Id])),
                N'Buyer Contracts', COALESCE(broker.[GroupName], N'Operations'), N'/BuyerContracts', 44
            FROM [Companies] company
            OUTER APPLY (SELECT TOP (1) [ModuleId], [GroupName] FROM [MenuItems] WHERE [CompanyId] = company.[Id] AND [Path] = N'/Brokers' ORDER BY [Id]) broker
            WHERE NOT EXISTS (SELECT 1 FROM [MenuItems] WHERE [CompanyId] = company.[Id] AND [Path] = N'/BuyerContracts')
                AND COALESCE(broker.[ModuleId], (SELECT MIN(module.[Id]) FROM [Modules] module WHERE module.[CompanyId] = company.[Id])) IS NOT NULL;

            INSERT INTO [MenuItemOperations] ([MenuItemId], [Key], [Name], [SortOrder])
            SELECT item.[Id], operation.[Key], operation.[Name], operation.[SortOrder]
            FROM [MenuItems] item CROSS JOIN (VALUES (N'create', N'Create', 0), (N'edit', N'Edit', 1), (N'delete', N'Delete', 2), (N'print', N'Print', 3)) operation([Key], [Name], [SortOrder])
            WHERE item.[Path] = N'/BuyerContracts' AND NOT EXISTS (SELECT 1 FROM [MenuItemOperations] existing WHERE existing.[MenuItemId] = item.[Id] AND existing.[Key] = operation.[Key]);

            INSERT INTO [MenuItemRoles] ([MenuItemId], [RoleId])
            SELECT DISTINCT target.[Id], assignment.[RoleId] FROM [MenuItems] target
            INNER JOIN [MenuItems] broker ON broker.[CompanyId] = target.[CompanyId] AND broker.[Path] = N'/Brokers'
            INNER JOIN [MenuItemRoles] assignment ON assignment.[MenuItemId] = broker.[Id]
            WHERE target.[Path] = N'/BuyerContracts' AND NOT EXISTS (SELECT 1 FROM [MenuItemRoles] existing WHERE existing.[MenuItemId] = target.[Id] AND existing.[RoleId] = assignment.[RoleId]);

            INSERT INTO [RoleOperations] ([RoleId], [OperationId])
            SELECT DISTINCT grantRow.[RoleId], targetOperation.[Id] FROM [MenuItems] target
            INNER JOIN [MenuItemOperations] targetOperation ON targetOperation.[MenuItemId] = target.[Id]
            INNER JOIN [MenuItems] broker ON broker.[CompanyId] = target.[CompanyId] AND broker.[Path] = N'/Brokers'
            INNER JOIN [MenuItemOperations] brokerOperation ON brokerOperation.[MenuItemId] = broker.[Id] AND brokerOperation.[Key] = targetOperation.[Key]
            INNER JOIN [RoleOperations] grantRow ON grantRow.[OperationId] = brokerOperation.[Id]
            WHERE target.[Path] = N'/BuyerContracts'
                AND EXISTS (SELECT 1 FROM [MenuItemRoles] menuRole WHERE menuRole.[MenuItemId] = target.[Id] AND menuRole.[RoleId] = grantRow.[RoleId])
                AND NOT EXISTS (SELECT 1 FROM [RoleOperations] existing WHERE existing.[RoleId] = grantRow.[RoleId] AND existing.[OperationId] = targetOperation.[Id]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DELETE FROM [MenuItems] WHERE [Path] = N'/BuyerContracts';");
}
