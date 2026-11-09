using HRMS.API.DTOs;

namespace HRMS.API.Interfaces;

public interface ILeaveService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync();
    Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(int employeeId);
    Task<LeaveBalanceDto?> GetLeaveBalanceAsync(int employeeId);
    Task<LeaveRequestDto> ApplyLeaveAsync(ApplyLeaveDto dto);
    Task<bool> UpdateLeaveStatusAsync(int leaveId, UpdateLeaveStatusDto dto);
}
