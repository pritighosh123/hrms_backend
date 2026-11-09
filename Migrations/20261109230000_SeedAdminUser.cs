using HRMS.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.API.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109230000_SeedAdminUser")]
public partial class SeedAdminUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM [Users]
    WHERE UPPER(LTRIM(RTRIM([Role]))) = N'ADMIN'
)
BEGIN
    DECLARE @AdminEmployeeId int;

    SELECT @AdminEmployeeId = [EmployeeId]
    FROM [Employees]
    WHERE [Email] = N'admin@gmail.com';

    IF @AdminEmployeeId IS NULL
    BEGIN
        INSERT INTO [Employees] ([Name], [Email], [Phone], [DepartmentId], [JoiningDate])
        VALUES (N'Admin', N'admin@gmail.com', N'1234567890', 1, SYSUTCDATETIME());

        SET @AdminEmployeeId = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE IF EXISTS (
        SELECT 1 FROM [Users] WHERE [EmployeeId] = @AdminEmployeeId
    )
    BEGIN
        THROW 51000, 'Employee admin@gmail.com is already linked to another user.', 1;
    END;

    INSERT INTO [Users] ([Username], [PasswordHash], [Role], [MustChangePassword], [EmployeeId])
    VALUES (
        N'Admin',
        N'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=',
        N'Admin',
        0,
        @AdminEmployeeId
    );
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DECLARE @AdminEmployeeId int;

SELECT @AdminEmployeeId = [EmployeeId]
FROM [Users]
WHERE [Username] = N'Admin'
  AND [Role] = N'Admin'
  AND [PasswordHash] = N'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=';

DELETE FROM [Users]
WHERE [Username] = N'Admin'
  AND [Role] = N'Admin'
  AND [PasswordHash] = N'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=';

IF @AdminEmployeeId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [Users] WHERE [EmployeeId] = @AdminEmployeeId)
   AND NOT EXISTS (SELECT 1 FROM [Attendance] WHERE [EmployeeId] = @AdminEmployeeId)
   AND NOT EXISTS (SELECT 1 FROM [LeaveRequests] WHERE [EmployeeId] = @AdminEmployeeId)
   AND NOT EXISTS (SELECT 1 FROM [LeaveBalances] WHERE [EmployeeId] = @AdminEmployeeId)
BEGIN
    DELETE FROM [Employees]
    WHERE [EmployeeId] = @AdminEmployeeId
      AND [Email] = N'admin@gmail.com'
      AND [Name] = N'Admin'
      AND [Phone] = N'1234567890';
END
");
    }
}