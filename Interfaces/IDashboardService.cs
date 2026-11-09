using HRMS.API.DTOs;

namespace HRMS.API.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IEnumerable<DepartmentDto>> GetDepartmentSummariesAsync();
}
