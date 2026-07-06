using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPO_Workflow.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CalendarWeek_isDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WorkingHours",
                table: "CalendarExceptions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "CalendarExceptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "CalendarExceptions");

            migrationBuilder.AlterColumn<int>(
                name: "WorkingHours",
                table: "CalendarExceptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
