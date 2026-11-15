using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CompanyId_Name",
                table: "Modules",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO [Modules] ([CompanyId], [Name])
                SELECT [Id], N'Blend' FROM [Companies];
                """);

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "MenuItems",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE menuItem
                SET [ModuleId] = module.[Id]
                FROM [MenuItems] AS menuItem
                INNER JOIN [Modules] AS module ON module.[CompanyId] = menuItem.[CompanyId] AND module.[Name] = N'Blend';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "MenuItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ModuleId",
                table: "MenuItems",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Modules_ModuleId",
                table: "MenuItems",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Modules_ModuleId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ModuleId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "MenuItems");
        }
    }
}
