using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPO_Workflow.DAL.Migrations
{
    /// <inheritdoc />
    public partial class isDeletedToAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "TechnologyOperations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Technologies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Details",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "TechnologyOperations");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Technologies");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Details");
        }
    }
}
