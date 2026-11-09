using System.Security.Cryptography;
using System.Text;
using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services;

public class EmployeeService : IEmployeeService
{
    private const string DefaultPasswordHash = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=";
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.LeaveBalance)
            .Include(e => e.User)
            .Include(e => e.ReportsTo)
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                JoiningDate = e.JoiningDate,
                TotalLeaves = e.LeaveBalance != null ? e.LeaveBalance.TotalLeaves : 0,
                UsedLeaves = e.LeaveBalance != null ? e.LeaveBalance.UsedLeaves : 0,
                RemainingLeaves = e.LeaveBalance != null ? e.LeaveBalance.RemainingLeaves : 0,
                UserId = e.User != null ? e.User.UserId : null,
                Username = e.User != null ? e.User.Username : null,
                Role = e.User != null ? e.User.Role : null,
                ReportsToEmployeeId = e.ReportsToEmployeeId,
                ReportsToEmployeeName = e.ReportsTo != null ? e.ReportsTo.Name : null
            })
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.LeaveBalance)
            .Include(e => e.User)
            .Include(e => e.ReportsTo)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null) return null;

        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.DepartmentName,
            JoiningDate = employee.JoiningDate,
            TotalLeaves = employee.LeaveBalance?.TotalLeaves,
            UsedLeaves = employee.LeaveBalance?.UsedLeaves,
            RemainingLeaves = employee.LeaveBalance?.RemainingLeaves,
            UserId = employee.User?.UserId,
            Username = employee.User?.Username,
            Role = employee.User?.Role,
            ReportsToEmployeeId = employee.ReportsToEmployeeId,
            ReportsToEmployeeName = employee.ReportsTo?.Name
        };
    }

    public async Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(int managerEmployeeId)
    {
        return await _context.Employees
            .Include(employee => employee.Department)
            .Include(employee => employee.LeaveBalance)
            .Include(employee => employee.User)
            .Where(employee => employee.ReportsToEmployeeId == managerEmployeeId)
            .OrderBy(employee => employee.Name)
            .Select(employee => new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Email = employee.Email,
                Phone = employee.Phone,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department != null ? employee.Department.DepartmentName : null,
                JoiningDate = employee.JoiningDate,
                TotalLeaves = employee.LeaveBalance != null ? employee.LeaveBalance.TotalLeaves : 0,
                UsedLeaves = employee.LeaveBalance != null ? employee.LeaveBalance.UsedLeaves : 0,
                RemainingLeaves = employee.LeaveBalance != null ? employee.LeaveBalance.RemainingLeaves : 0,
                UserId = employee.User != null ? employee.User.UserId : null,
                Username = employee.User != null ? employee.User.Username : null,
                Role = employee.User != null ? employee.User.Role : null,
                ReportsToEmployeeId = employee.ReportsToEmployeeId,
                ReportsToEmployeeName = employee.ReportsTo != null ? employee.ReportsTo.Name : null
            })
            .ToListAsync();
    }

    public async Task<bool> RevokeSubordinateAsync(int managerEmployeeId, int employeeId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(item =>
                item.EmployeeId == employeeId &&
                item.ReportsToEmployeeId == managerEmployeeId);

        if (employee == null) return false;

        employee.ReportsToEmployeeId = null;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var email = dto.Email.Trim();

        var existingEmail = await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == email.ToLower());
        if (existingEmail)
        {
            throw new InvalidOperationException($"An employee with email '{email}' already exists.");
        }

        var username = string.IsNullOrWhiteSpace(dto.Username) ? email : dto.Username.Trim();
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        if (existingUser)
        {
            throw new InvalidOperationException($"A user with username '{username}' already exists.");
        }

        if (dto.ReportsToEmployeeId.HasValue)
        {
            var managerExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == dto.ReportsToEmployeeId.Value);
            if (!managerExists)
            {
                throw new InvalidOperationException($"The selected manager (Employee #{dto.ReportsToEmployeeId.Value}) does not exist.");
            }
        }

        var passwordHash = string.IsNullOrWhiteSpace(dto.Password)
            ? DefaultPasswordHash
            : (dto.Password == DefaultPasswordHash ? dto.Password : HashPassword(dto.Password));

        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role.Trim();

        var employee = new Employee
        {
            Name = dto.Name.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim() ?? string.Empty,
            DepartmentId = dto.DepartmentId,
            JoiningDate = dto.JoiningDate,
            ReportsToEmployeeId = dto.ReportsToEmployeeId,
            LeaveBalance = new LeaveBalance
            {
                TotalLeaves = dto.InitialTotalLeaves,
                UsedLeaves = 0
            },
            User = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = role,
                MustChangePassword = !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            }
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Load navigation properties for response
        if (employee.DepartmentId.HasValue)
        {
            await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
        }

        if (employee.ReportsToEmployeeId.HasValue)
        {
            await _context.Entry(employee).Reference(e => e.ReportsTo).LoadAsync();
        }

        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.DepartmentName,
            JoiningDate = employee.JoiningDate,
            TotalLeaves = employee.LeaveBalance?.TotalLeaves,
            UsedLeaves = employee.LeaveBalance?.UsedLeaves,
            RemainingLeaves = employee.LeaveBalance?.RemainingLeaves,
            UserId = employee.User?.UserId,
            Username = employee.User?.Username,
            Role = employee.User?.Role,
            ReportsToEmployeeId = employee.ReportsToEmployeeId,
            ReportsToEmployeeName = employee.ReportsTo?.Name
        };
    }

    public async Task<bool> AssignManagerAsync(int employeeId, int managerEmployeeId)
    {
        var employee = await _context.Employees
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.EmployeeId == employeeId);

        if (employee == null ||
            string.Equals(employee.User?.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            employee.ReportsToEmployeeId.HasValue ||
            employeeId == managerEmployeeId)
        {
            return false;
        }

        var managerRole = await _context.Employees
            .Where(item => item.EmployeeId == managerEmployeeId)
            .Select(item => item.User == null ? null : item.User.Role)
            .FirstOrDefaultAsync();

        if (!string.Equals(managerRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        employee.ReportsToEmployeeId = managerEmployeeId;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null) return false;

        var oldEmail = employee.Email;
        var newEmail = dto.Email.Trim();

        if (!string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailInUse = await _context.Employees
                .AnyAsync(e => e.EmployeeId != id && e.Email.ToLower() == newEmail.ToLower());
            if (emailInUse)
            {
                throw new InvalidOperationException($"An employee with email '{newEmail}' already exists.");
            }

            if (employee.User != null && string.Equals(employee.User.Username, oldEmail, StringComparison.OrdinalIgnoreCase))
            {
                var usernameInUse = await _context.Users
                    .AnyAsync(u => u.UserId != employee.User.UserId && u.Username.ToLower() == newEmail.ToLower());
                if (!usernameInUse)
                {
                    employee.User.Username = newEmail;
                }
            }
        }

        if (dto.ReportsToEmployeeId.HasValue)
        {
            if (dto.ReportsToEmployeeId.Value == id)
            {
                throw new InvalidOperationException("An employee cannot report to themselves.");
            }

            var managerExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == dto.ReportsToEmployeeId.Value);
            if (!managerExists)
            {
                throw new InvalidOperationException($"The selected manager (Employee #{dto.ReportsToEmployeeId.Value}) does not exist.");
            }
        }

        employee.Name = dto.Name.Trim();
        employee.Email = newEmail;
        employee.Phone = dto.Phone?.Trim() ?? string.Empty;
        employee.DepartmentId = dto.DepartmentId;
        employee.ReportsToEmployeeId = dto.ReportsToEmployeeId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null) return false;

        // Clear ReportsTo for any subordinates before deleting this employee
        var subordinates = await _context.Employees
            .Where(e => e.ReportsToEmployeeId == id)
            .ToListAsync();
        foreach (var sub in subordinates)
        {
            sub.ReportsToEmployeeId = null;
        }

        if (employee.User != null)
        {
            _context.Users.Remove(employee.User);
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
