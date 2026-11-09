using System.ComponentModel.DataAnnotations;

namespace HRMS.API.DTOs;

public class AttendanceDto
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MarkAttendanceDto
{
    [Required]
    public int EmployeeId { get; set; }

    public DateTime? Date { get; set; }

    [Required]
    [RegularExpression("^(Present|Absent|HalfDay|OnLeave)$", ErrorMessage = "Status must be 'Present', 'Absent', 'HalfDay', or 'OnLeave'.")]
    public string Status { get; set; } = "Present";
}
