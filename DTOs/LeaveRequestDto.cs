using System.ComponentModel.DataAnnotations;

namespace HRMS.API.DTOs;

public class LeaveRequestDto
{
    public int LeaveId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? RejectionDetails { get; set; }
}

public class ApplyLeaveDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Range(1, 365)]
    public int TotalDays { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Details { get; set; } = string.Empty;
}

public class UpdateLeaveStatusDto
{
    [Required]
    [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status must be 'Approved' or 'Rejected'.")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? RejectionDetails { get; set; }
}

public class LeaveBalanceDto
{
    public int BalanceId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }
    public int RemainingLeaves => TotalLeaves - UsedLeaves;
}