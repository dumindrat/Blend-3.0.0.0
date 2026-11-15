using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class UseFourDigitCompanyNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF (SELECT COUNT_BIG(*) FROM [Companies]) > 9000
                    THROW 51000, 'Four-digit company numbers support at most 9,000 companies.', 1;

                DROP INDEX [IX_Companies_CompanyNumber] ON [Companies];
                ;WITH numbered AS
                (
                    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS [RowNumber]
                    FROM [Companies]
                )
                UPDATE company
                SET [CompanyNumber] = -numbered.[RowNumber]
                FROM [Companies] AS company
                INNER JOIN numbered ON numbered.[Id] = company.[Id];

                DECLARE @CompanyId uniqueidentifier;
                DECLARE @CompanyNumber int;
                DECLARE CompanyCursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [Id] FROM [Companies] ORDER BY NEWID();

                OPEN CompanyCursor;
                FETCH NEXT FROM CompanyCursor INTO @CompanyId;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    WHILE 1 = 1
                    BEGIN
                        SET @CompanyNumber = 1000 + CONVERT(int, ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 9000);
                        IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [CompanyNumber] = @CompanyNumber)
                            BREAK;
                    END;

                    UPDATE [Companies] SET [CompanyNumber] = @CompanyNumber WHERE [Id] = @CompanyId;
                    FETCH NEXT FROM CompanyCursor INTO @CompanyId;
                END;
                CLOSE CompanyCursor;
                DEALLOCATE CompanyCursor;

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
                    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS [RowNumber]
                    FROM [Companies]
                )
                UPDATE company
                SET [CompanyNumber] = -numbered.[RowNumber]
                FROM [Companies] AS company
                INNER JOIN numbered ON numbered.[Id] = company.[Id];

                DECLARE @CompanyId uniqueidentifier;
                DECLARE @CompanyNumber int;
                DECLARE CompanyCursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [Id] FROM [Companies] ORDER BY NEWID();

                OPEN CompanyCursor;
                FETCH NEXT FROM CompanyCursor INTO @CompanyId;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    WHILE 1 = 1
                    BEGIN
                        SET @CompanyNumber = 100000000 + CONVERT(int, ABS(CONVERT(bigint, CHECKSUM(NEWID()))) % 2000000000);
                        IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [CompanyNumber] = @CompanyNumber)
                            BREAK;
                    END;

                    UPDATE [Companies] SET [CompanyNumber] = @CompanyNumber WHERE [Id] = @CompanyId;
                    FETCH NEXT FROM CompanyCursor INTO @CompanyId;
                END;
                CLOSE CompanyCursor;
                DEALLOCATE CompanyCursor;

                CREATE UNIQUE INDEX [IX_Companies_CompanyNumber] ON [Companies] ([CompanyNumber]);
                """);
        }
    }
}
