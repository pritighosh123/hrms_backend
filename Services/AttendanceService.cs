using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;

    public AttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Date.Date == targetDate)
            .Select(a => new AttendanceDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.Name : null,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceByEmployeeAsync(int employeeId)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Date)
            .Select(a => new AttendanceDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? a.Employee.Name : null,
                Date = a.Date,
                Status = a.Status
            })
            .ToListAsync();
    }

    public async Task<AttendanceDto> MarkAttendanceAsync(MarkAttendanceDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} not found.");

        var recordDate = (dto.Date ?? DateTime.UtcNow).Date;

        var existingRecord = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.Date.Date == recordDate);

        if (existingRecord != null)
        {
            existingRecord.Status = dto.Status;
            await _context.SaveChangesAsync();

            return new AttendanceDto
            {
                AttendanceId = existingRecord.AttendanceId,
                EmployeeId = existingRecord.EmployeeId,
                EmployeeName = employee.Name,
                Date = existingRecord.Date,
                Status = existingRecord.Status
            };
        }

        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            Date = recordDate,
            Status = dto.Status
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return new AttendanceDto
        {
            AttendanceId = attendance.AttendanceId,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = employee.Name,
            Date = attendance.Date,
            Status = attendance.Status
        };
    }
}
