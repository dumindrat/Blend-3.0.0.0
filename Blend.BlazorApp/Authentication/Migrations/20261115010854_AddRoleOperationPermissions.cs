using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blend.BlazorApp.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleOperationPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItemOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemOperations_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleOperations",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleOperations", x => new { x.RoleId, x.OperationId });
                    table.ForeignKey(
                        name: "FK_RoleOperations_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleOperations_MenuItemOperations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "MenuItemOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemOperations_MenuItemId_Key",
                table: "MenuItemOperations",
                columns: new[] { "MenuItemId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleOperations_OperationId",
                table: "RoleOperations",
                column: "OperationId");

            migrationBuilder.Sql("""
                INSERT INTO [MenuItemOperations] ([MenuItemId], [Key], [Name], [SortOrder])
                SELECT menuItem.[Id], operation.[Key], operation.[Name], operation.[SortOrder]
                FROM [MenuItems] AS menuItem
                CROSS JOIN (VALUES
                    (N'create', N'Create', 0),
                    (N'edit', N'Edit', 1),
                    (N'delete', N'Delete', 2),
                    (N'print', N'Print', 3)
                ) AS operation([Key], [Name], [SortOrder]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleOperations");

            migrationBuilder.DropTable(
                name: "MenuItemOperations");
        }
    }
}
