using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;

        var totalEmployees = await _context.Employees.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();

        var todayAttendance = await _context.Attendances
            .Where(a => a.Date.Date == today)
            .ToListAsync();

        var presentToday = todayAttendance.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
        var absentToday = todayAttendance.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
        var onLeaveToday = todayAttendance.Count(a => a.Status.Equals("OnLeave", StringComparison.OrdinalIgnoreCase));

        var pendingLeaveRequests = await _context.LeaveRequests
            .CountAsync(l => l.Status == "Pending");

        return new DashboardSummaryDto
        {
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            OnLeaveToday = onLeaveToday,
            PendingLeaveRequests = pendingLeaveRequests
        };
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentSummariesAsync()
    {
        return await _context.Departments
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                EmployeeCount = d.Employees.Count
            })
            .ToListAsync();
    }
}
