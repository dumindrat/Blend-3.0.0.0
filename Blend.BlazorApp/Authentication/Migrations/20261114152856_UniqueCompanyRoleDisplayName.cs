using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class UniqueCompanyRoleDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CompanyId",
                table: "AspNetRoles");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyId_DisplayName",
                table: "AspNetRoles",
                columns: new[] { "CompanyId", "DisplayName" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CompanyId_DisplayName",
                table: "AspNetRoles");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyId",
                table: "AspNetRoles",
                column: "CompanyId");
        }
    }
}
