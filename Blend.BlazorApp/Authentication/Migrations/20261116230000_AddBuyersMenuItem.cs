using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    [DbContext(typeof(BlendIdentityDbContext))]
    [Migration("20261116230000_AddBuyersMenuItem")]
    public partial class AddBuyersMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mirrors each company's Brokers menu item (module and group); falls back to the company's first module.
            migrationBuilder.Sql("""
                INSERT INTO [MenuItems] ([CompanyId], [ModuleId], [Name], [GroupName], [Path], [SortOrder])
                SELECT company.[Id],
                    COALESCE(broker.[ModuleId], (SELECT MIN(module.[Id]) FROM [Modules] AS module WHERE module.[CompanyId] = company.[Id])),
                    N'Buyers',
                    COALESCE(broker.[GroupName], N'Operations'),
                    N'/Buyers',
                    41
                FROM [Companies] AS company
                OUTER APPLY (
                    SELECT TOP (1) [ModuleId], [GroupName] FROM [MenuItems]
                    WHERE [CompanyId] = company.[Id] AND [Path] = N'/Brokers' ORDER BY [Id]
                ) AS broker
                WHERE NOT EXISTS (SELECT 1 FROM [MenuItems] WHERE [CompanyId] = company.[Id] AND [Path] = N'/Buyers')
                    AND COALESCE(broker.[ModuleId], (SELECT MIN(module.[Id]) FROM [Modules] AS module WHERE module.[CompanyId] = company.[Id])) IS NOT NULL;

                INSERT INTO [MenuItemOperations] ([MenuItemId], [Key], [Name], [SortOrder])
                SELECT menuItem.[Id], operation.[Key], operation.[Name], operation.[SortOrder]
                FROM [MenuItems] AS menuItem
                CROSS JOIN (VALUES
                    (N'create', N'Create', 0),
                    (N'edit', N'Edit', 1),
                    (N'delete', N'Delete', 2),
                    (N'print', N'Print', 3)
                ) AS operation([Key], [Name], [SortOrder])
                WHERE menuItem.[Path] = N'/Buyers'
                    AND NOT EXISTS (SELECT 1 FROM [MenuItemOperations] AS existing WHERE existing.[MenuItemId] = menuItem.[Id] AND existing.[Key] = operation.[Key]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [MenuItems] WHERE [Path] = N'/Buyers';");
        }
    }
}
