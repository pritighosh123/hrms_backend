using HRMS.API.DTOs;

namespace HRMS.API.Interfaces;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(DateTime date);
    Task<IEnumerable<AttendanceDto>> GetAttendanceByEmployeeAsync(int employeeId);
    Task<AttendanceDto> MarkAttendanceAsync(MarkAttendanceDto dto);
}
