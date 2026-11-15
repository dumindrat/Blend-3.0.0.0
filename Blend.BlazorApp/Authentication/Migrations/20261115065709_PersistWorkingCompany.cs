using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class PersistWorkingCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkingCompanyId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE [AspNetUsers] SET [WorkingCompanyId] = [CompanyId] WHERE [CompanyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingCompanyId",
                table: "AspNetUsers");
        }
    }
}
