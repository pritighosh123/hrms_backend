using System.Security.Claims;
using System.Text.RegularExpressions;
using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ApplicationDbContext _context;

    public EmployeeController(IEmployeeService employeeService, ApplicationDbContext context)
    {
        _employeeService = employeeService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }
        return Ok(employee);
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("subordinates")]
    public async Task<IActionResult> GetMySubordinates()
    {
        var managerEmployeeId = await GetCurrentEmployeeIdAsync();
        if (!managerEmployeeId.HasValue)
        {
            return Forbid();
        }

        var employees = await _employeeService.GetSubordinatesAsync(managerEmployeeId.Value);
        return Ok(employees);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("subordinates/{id:int}/revoke")]
    public async Task<IActionResult> RevokeSubordinate(int id)
    {
        var managerEmployeeId = await GetCurrentEmployeeIdAsync();
        if (!managerEmployeeId.HasValue)
        {
            return Forbid();
        }

        var revoked = await _employeeService.RevokeSubordinateAsync(managerEmployeeId.Value, id);
        if (!revoked)
        {
            return NotFound(new { message = "Employee not found in your direct reports." });
        }

        return Ok(new { message = "Employee removed from your team." });
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/manager")]
    public async Task<IActionResult> AssignEmployeeToManager(int id, [FromBody] AssignManagerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employee = await _context.Employees
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.EmployeeId == id);
        if (employee == null)
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }

        if (string.Equals(employee.User?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Admin employees cannot be assigned to a manager." });
        }

        if (employee.ReportsToEmployeeId.HasValue)
        {
            return Conflict(new { message = "This employee is already assigned to a manager." });
        }

        if (dto.ManagerEmployeeId == id)
        {
            return BadRequest(new { message = "An employee cannot report to themselves." });
        }

        var managerRole = await _context.Employees
            .Where(item => item.EmployeeId == dto.ManagerEmployeeId)
            .Select(item => item.User == null ? null : item.User.Role)
            .FirstOrDefaultAsync();
        if (!string.Equals(managerRole, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Select an employee with the Manager role." });
        }

        var assigned = await _employeeService.AssignManagerAsync(id, dto.ManagerEmployeeId!.Value);
        if (!assigned)
        {
            return Conflict(new { message = "The employee could not be assigned to this manager." });
        }

        return Ok(new { message = "Employee assigned to manager successfully." });
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!IsValidMobileNumber(dto.Phone))
        {
            return BadRequest(new { message = "Enter a valid phone number with 7 to 15 digits." });
        }

        if (!dto.ReportsToEmployeeId.HasValue)
        {
            return BadRequest(new { message = "Report To (Manager) is required." });
        }

        var created = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId }, created);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!IsValidMobileNumber(dto.Phone))
        {
            return BadRequest(new { message = "Enter a valid phone number with 7 to 15 digits." });
        }

        var employeeRole = await _context.Employees
            .Where(employee => employee.EmployeeId == id)
            .Select(employee => employee.User == null ? null : employee.User.Role)
            .FirstOrDefaultAsync();

        if (employeeRole == null && !await _context.Employees.AnyAsync(employee => employee.EmployeeId == id))
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }

        if (!string.Equals(employeeRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !dto.ReportsToEmployeeId.HasValue)
        {
            return BadRequest(new { message = "Report To (Manager) is required for non-Admin employees." });
        }

        var updated = await _employeeService.UpdateEmployeeAsync(id, dto);
        if (!updated)
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }

        return Ok(new { message = "Employee updated successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await GetCurrentEmployeeIdAsync() == id)
        {
            return BadRequest(new { message = "You cannot delete your own Admin account." });
        }

        var deleted = await _employeeService.DeleteEmployeeAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }

        return Ok(new { message = "Employee deleted successfully." });
    }

    private async Task<int?> GetCurrentEmployeeIdAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return await _context.Users
                .Where(user => user.UserId == userId)
                .Select(user => user.EmployeeId)
                .FirstOrDefaultAsync();
        }

        return int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId)
            ? employeeId
            : null;
    }

    private static bool IsValidMobileNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        var value = phone.Trim();
        var digitCount = Regex.Matches(value, @"\d").Count;
        return Regex.IsMatch(value, @"^\+?[\d\s().-]+$") && digitCount >= 7 && digitCount <= 15;
    }
}