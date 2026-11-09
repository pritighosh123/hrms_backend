using HRMS.API.Data;
using System.Security.Claims;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly ApplicationDbContext _context;

    public LeaveController(ILeaveService leaveService, ApplicationDbContext context)
    {
        _leaveService = leaveService;
        _context = context;
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpGet("requests")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _leaveService.GetAllLeaveRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("requests/employee/{employeeId:int}")]
    public async Task<IActionResult> GetRequestsByEmployee(int employeeId)
    {
        var requests = await _leaveService.GetLeaveRequestsByEmployeeAsync(employeeId);
        return Ok(requests);
    }

    [HttpGet("balance/{employeeId:int}")]
    public async Task<IActionResult> GetBalance(int employeeId)
    {
        var balance = await _leaveService.GetLeaveBalanceAsync(employeeId);
        if (balance == null)
        {
            return NotFound(new { message = $"Leave balance for employee {employeeId} not found." });
        }
        return Ok(balance);
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(dto.Details))
        {
            return BadRequest(new { message = "Leave details are required." });
        }

        var result = await _leaveService.ApplyLeaveAsync(dto);
        return Ok(result);
    }

    [HttpPut("requests/{leaveId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int leaveId, [FromBody] UpdateLeaveStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (dto.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(dto.RejectionDetails))
        {
            return BadRequest(new { message = "Rejection details are required." });
        }

        if (!User.IsInRole("Admin"))
        {
            int? currentEmployeeId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var currentUserId))
            {
                currentEmployeeId = await _context.Users
                    .Where(user => user.UserId == currentUserId)
                    .Select(user => user.EmployeeId)
                    .FirstOrDefaultAsync();
            }

            if (!currentEmployeeId.HasValue &&
                int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeIdFromClaim))
            {
                currentEmployeeId = employeeIdFromClaim;
            }

            if (!currentEmployeeId.HasValue)
            {
                return Forbid();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(request => request.Employee)
                .FirstOrDefaultAsync(request => request.LeaveId == leaveId);

            if (leaveRequest == null)
            {
                return NotFound(new { message = $"Leave request with ID {leaveId} not found." });
            }

            if (leaveRequest.Employee?.ReportsToEmployeeId != currentEmployeeId.Value)
            {
                return Forbid();
            }
        }

        var updated = await _leaveService.UpdateLeaveStatusAsync(leaveId, dto);
        if (!updated)
        {
            return NotFound(new { message = $"Leave request with ID {leaveId} not found." });
        }

        return Ok(new { message = $"Leave request {leaveId} has been updated to {dto.Status}." });
    }
}