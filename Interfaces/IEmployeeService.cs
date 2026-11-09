using HRMS.API.DTOs;

namespace HRMS.API.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(int managerEmployeeId);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
        Task<bool> RevokeSubordinateAsync(int managerEmployeeId, int employeeId);
        Task<bool> AssignManagerAsync(int employeeId, int managerEmployeeId);
        Task<bool> DeleteEmployeeAsync(int id);
    }
}