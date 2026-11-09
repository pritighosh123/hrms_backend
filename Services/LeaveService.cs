using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services;

public class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _context;

    public LeaveService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .OrderByDescending(l => l.FromDate)
            .Select(l => new LeaveRequestDto
            {
                LeaveId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee != null ? l.Employee.Name : null,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                TotalDays = l.TotalDays,
                Status = l.Status,
                Details = l.Details,
                RejectionDetails = l.RejectionDetails
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.FromDate)
            .Select(l => new LeaveRequestDto
            {
                LeaveId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee != null ? l.Employee.Name : null,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                TotalDays = l.TotalDays,
                Status = l.Status,
                Details = l.Details,
                RejectionDetails = l.RejectionDetails
            })
            .ToListAsync();
    }

    public async Task<LeaveBalanceDto?> GetLeaveBalanceAsync(int employeeId)
    {
        var balance = await _context.LeaveBalances
            .Include(b => b.Employee)
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId);

        if (balance == null) return null;

        return new LeaveBalanceDto
        {
            BalanceId = balance.BalanceId,
            EmployeeId = balance.EmployeeId,
            EmployeeName = balance.Employee?.Name,
            TotalLeaves = balance.TotalLeaves,
            UsedLeaves = balance.UsedLeaves
        };
    }

    public async Task<LeaveRequestDto> ApplyLeaveAsync(ApplyLeaveDto dto)
    {
        var employee = await _context.Employees
            .Include(e => e.LeaveBalance)
            .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} not found.");

        if (dto.ToDate < dto.FromDate)
        {
            throw new ArgumentException("To Date cannot be earlier than From Date.");
        }

        var hasOverlappingLeave = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == dto.EmployeeId
                        && l.Status != "Rejected"
                        && l.FromDate.Date <= dto.ToDate.Date
                        && l.ToDate.Date >= dto.FromDate.Date);

        if (hasOverlappingLeave)
        {
            throw new InvalidOperationException($"A leave request already exists for employee ID {dto.EmployeeId} in the selected date range ({dto.FromDate:yyyy-MM-dd} to {dto.ToDate:yyyy-MM-dd}).");
        }

        if (employee.LeaveBalance != null && employee.LeaveBalance.RemainingLeaves < dto.TotalDays)
        {
            throw new InvalidOperationException($"Insufficient leave balance. Remaining: {employee.LeaveBalance.RemainingLeaves}, Requested: {dto.TotalDays}.");
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            TotalDays = dto.TotalDays,
            Status = "Pending",
            Details = dto.Details.Trim()
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        return new LeaveRequestDto
        {
            LeaveId = leaveRequest.LeaveId,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employee.Name,
            FromDate = leaveRequest.FromDate,
            ToDate = leaveRequest.ToDate,
            TotalDays = leaveRequest.TotalDays,
            Status = leaveRequest.Status,
            Details = leaveRequest.Details,
            RejectionDetails = leaveRequest.RejectionDetails
        };
    }

    public async Task<bool> UpdateLeaveStatusAsync(int leaveId, UpdateLeaveStatusDto dto)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .ThenInclude(e => e!.LeaveBalance)
            .FirstOrDefaultAsync(l => l.LeaveId == leaveId);

        if (leaveRequest == null) return false;

        // If transitioning to Approved from Pending, deduct from balance
        if (dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
            !leaveRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (leaveRequest.Employee?.LeaveBalance != null)
            {
                leaveRequest.Employee.LeaveBalance.UsedLeaves += leaveRequest.TotalDays;
            }
        }
        // If transitioning from Approved to Rejected, restore balance
        else if (dto.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) &&
                 leaveRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (leaveRequest.Employee?.LeaveBalance != null)
            {
                leaveRequest.Employee.LeaveBalance.UsedLeaves = Math.Max(0, leaveRequest.Employee.LeaveBalance.UsedLeaves - leaveRequest.TotalDays);
            }
        }

        leaveRequest.Status = dto.Status;
        leaveRequest.RejectionDetails = dto.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase)
            ? dto.RejectionDetails?.Trim()
            : null;
        await _context.SaveChangesAsync();
        return true;
    }
}
