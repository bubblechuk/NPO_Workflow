using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPO_Workflow.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TechOpsHierarchyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HierarchyId",
                table: "TechnologyOperations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HierarchyId",
                table: "TechnologyOperations");
        }
    }
}
