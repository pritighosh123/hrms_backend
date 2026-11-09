using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsToToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportsToEmployeeId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ReportsToEmployeeId",
                table: "Employees",
                column: "ReportsToEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_ReportsToEmployeeId",
                table: "Employees",
                column: "ReportsToEmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_ReportsToEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ReportsToEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ReportsToEmployeeId",
                table: "Employees");
        }
    }
}
