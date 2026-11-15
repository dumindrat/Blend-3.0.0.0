using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class UseOneDigitCompanyMappingIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Id] = 'F0C1A28D-7F0D-43A1-9CE8-9C3A6B63F274')
                    THROW 51000, 'The default company required for mapping ID 1 was not found.', 1;
                IF (SELECT COUNT_BIG(*) FROM [Companies]) > 3
                    THROW 51000, 'Company mapping IDs 1-3 support at most three companies.', 1;

                DROP INDEX [IX_Companies_CompanyNumber] ON [Companies];
                ;WITH numbered AS
                (
                    SELECT [Id], ROW_NUMBER() OVER (
                        ORDER BY CASE WHEN [Id] = 'F0C1A28D-7F0D-43A1-9CE8-9C3A6B63F274' THEN 0 ELSE 1 END, [Id]) AS [MappingId]
                    FROM [Companies]
                )
                UPDATE company
                SET [CompanyNumber] = CONVERT(int, numbered.[MappingId])
                FROM [Companies] AS company
                INNER JOIN numbered ON numbered.[Id] = company.[Id];

                CREATE UNIQUE INDEX [IX_Companies_CompanyNumber] ON [Companies] ([CompanyNumber]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX [IX_Companies_CompanyNumber] ON [Companies];
                ;WITH numbered AS
                (
                    SELECT [Id], ROW_NUMBER() OVER (
                        ORDER BY CASE WHEN [Id] = 'F0C1A28D-7F0D-43A1-9CE8-9C3A6B63F274' THEN 0 ELSE 1 END, [Id]) AS [RowNumber]
                    FROM [Companies]
                )
                UPDATE company
                SET [CompanyNumber] = 999 + CONVERT(int, numbered.[RowNumber])
                FROM [Companies] AS company
                INNER JOIN numbered ON numbered.[Id] = company.[Id];

                CREATE UNIQUE INDEX [IX_Companies_CompanyNumber] ON [Companies] ([CompanyNumber]);
                """);
        }
    }
}
