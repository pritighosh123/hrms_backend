using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRMS.API.Models;

[Table("LeaveRequests")]
public class LeaveRequest
{
    [Key]
    public int LeaveId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public Employee? Employee { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int TotalDays { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    [MaxLength(1000)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? RejectionDetails { get; set; }
}