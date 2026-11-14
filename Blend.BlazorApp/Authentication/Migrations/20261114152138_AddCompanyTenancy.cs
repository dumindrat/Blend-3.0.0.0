using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "AspNetRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetRoles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubscriptionStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SubscriptionEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            var defaultCompanyId = new Guid("F0C1A28D-7F0D-43A1-9CE8-9C3A6B63F274");
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Name", "SubscriptionStartDate", "SubscriptionEndDate", "IsEnabled" },
                values: new object[] { defaultCompanyId, "Default Company", new DateOnly(2000, 1, 1), new DateOnly(9999, 12, 31), true });
            migrationBuilder.Sql($"UPDATE [AspNetRoles] SET [DisplayName] = [Name], [CompanyId] = CASE WHEN [Name] = N'Admin' THEN NULL ELSE '{defaultCompanyId}' END");
            migrationBuilder.Sql($"UPDATE [AspNetUsers] SET [CompanyId] = CASE WHEN EXISTS (SELECT 1 FROM [AspNetUserRoles] ur INNER JOIN [AspNetRoles] r ON r.[Id] = ur.[RoleId] WHERE ur.[UserId] = [AspNetUsers].[Id] AND r.[Name] = N'Admin') THEN NULL ELSE '{defaultCompanyId}' END");
            migrationBuilder.Sql($"UPDATE [MenuItems] SET [CompanyId] = '{defaultCompanyId}'");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CompanyId",
                table: "MenuItems",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyId",
                table: "AspNetRoles",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_Companies_CompanyId",
                table: "AspNetRoles",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Companies_CompanyId",
                table: "MenuItems",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_Companies_CompanyId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Companies_CompanyId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_CompanyId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CompanyId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetRoles");
        }
    }
}
