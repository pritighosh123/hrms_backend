using System.ComponentModel.DataAnnotations;

namespace HRMS.API.DTOs;

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime JoiningDate { get; set; }
    public int? TotalLeaves { get; set; }
    public int? UsedLeaves { get; set; }
    public int? RemainingLeaves { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public int? ReportsToEmployeeId { get; set; }
    public string? ReportsToEmployeeName { get; set; }
}

public class CreateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

    public int InitialTotalLeaves { get; set; } = 24;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    [Required]
    public int? ReportsToEmployeeId { get; set; }
}

public class AssignManagerDto
{
    [Required]
    public int? ManagerEmployeeId { get; set; }
}
public class UpdateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public int? ReportsToEmployeeId { get; set; }
}