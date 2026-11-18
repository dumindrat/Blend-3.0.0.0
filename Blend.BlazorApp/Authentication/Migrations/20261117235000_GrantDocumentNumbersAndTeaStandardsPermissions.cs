using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    [DbContext(typeof(BlendIdentityDbContext))]
    [Migration("20261117235000_GrantDocumentNumbersAndTeaStandardsPermissions")]
    public partial class GrantDocumentNumbersAndTeaStandardsPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles that can use a company's Brokers page get the same access to the new pages, including matching operation grants.
            migrationBuilder.Sql("""
                INSERT INTO [MenuItemRoles] ([MenuItemId], [RoleId])
                SELECT target.[Id], assignment.[RoleId]
                FROM [MenuItems] AS target
                INNER JOIN [MenuItems] AS broker ON broker.[CompanyId] = target.[CompanyId] AND broker.[Path] = N'/Brokers'
                INNER JOIN [MenuItemRoles] AS assignment ON assignment.[MenuItemId] = broker.[Id]
                WHERE target.[Path] IN (N'/DocumentNumbers', N'/TeaStandards')
                    AND NOT EXISTS (SELECT 1 FROM [MenuItemRoles] AS existing WHERE existing.[MenuItemId] = target.[Id] AND existing.[RoleId] = assignment.[RoleId]);

                INSERT INTO [RoleOperations] ([RoleId], [OperationId])
                SELECT brokerGrant.[RoleId], targetOperation.[Id]
                FROM [MenuItems] AS target
                INNER JOIN [MenuItemOperations] AS targetOperation ON targetOperation.[MenuItemId] = target.[Id]
                INNER JOIN [MenuItems] AS broker ON broker.[CompanyId] = target.[CompanyId] AND broker.[Path] = N'/Brokers'
                INNER JOIN [MenuItemOperations] AS brokerOperation ON brokerOperation.[MenuItemId] = broker.[Id] AND brokerOperation.[Key] = targetOperation.[Key]
                INNER JOIN [RoleOperations] AS brokerGrant ON brokerGrant.[OperationId] = brokerOperation.[Id]
                WHERE target.[Path] IN (N'/DocumentNumbers', N'/TeaStandards')
                    AND EXISTS (SELECT 1 FROM [MenuItemRoles] AS menuRole WHERE menuRole.[MenuItemId] = target.[Id] AND menuRole.[RoleId] = brokerGrant.[RoleId])
                    AND NOT EXISTS (SELECT 1 FROM [RoleOperations] AS existing WHERE existing.[RoleId] = brokerGrant.[RoleId] AND existing.[OperationId] = targetOperation.[Id]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE grantRow FROM [RoleOperations] AS grantRow
                INNER JOIN [MenuItemOperations] AS operation ON operation.[Id] = grantRow.[OperationId]
                INNER JOIN [MenuItems] AS item ON item.[Id] = operation.[MenuItemId]
                WHERE item.[Path] IN (N'/DocumentNumbers', N'/TeaStandards');

                DELETE assignment FROM [MenuItemRoles] AS assignment
                INNER JOIN [MenuItems] AS item ON item.[Id] = assignment.[MenuItemId]
                WHERE item.[Path] IN (N'/DocumentNumbers', N'/TeaStandards');
                """);
        }
    }
}
