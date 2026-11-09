using HRMS.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.API.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109223000_AddLeaveDetails")]
public partial class AddLeaveDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Details",
            table: "LeaveRequests",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "RejectionDetails",
            table: "LeaveRequests",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Details", table: "LeaveRequests");
        migrationBuilder.DropColumn(name: "RejectionDetails", table: "LeaveRequests");
    }
}