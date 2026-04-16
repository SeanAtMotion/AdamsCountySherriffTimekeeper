using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timekeeping.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSupervisorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupervisorName",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupervisorName",
                table: "Employees");
        }
    }
}
