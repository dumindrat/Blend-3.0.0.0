using Blend.BlazorApp.Authentication;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations;

[DbContext(typeof(BlendIdentityDbContext))]
[Migration("20261122213000_AddCompanyDateFormat")]
public sealed class AddCompanyDateFormat : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
        name: "DateFormat",
        table: "Companies",
        type: "nvarchar(10)",
        maxLength: 10,
        nullable: false,
        defaultValue: "MM/dd/yyyy");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
        name: "DateFormat",
        table: "Companies");
}
