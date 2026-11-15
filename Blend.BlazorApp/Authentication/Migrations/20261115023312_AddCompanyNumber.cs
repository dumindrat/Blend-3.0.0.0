using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyNumber",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                DECLARE @CompanyId uniqueidentifier;
                DECLARE @CompanyNumber int;
                DECLARE CompanyCursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT [Id] FROM [Companies] ORDER BY [Id];

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
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CompanyNumber",
                table: "Companies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyNumber",
                table: "Companies",
                column: "CompanyNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_CompanyNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyNumber",
                table: "Companies");
        }
    }
}
