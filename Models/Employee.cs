using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRMS.API.Models;

[Table("Employees")]
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

    public int? ReportsToEmployeeId { get; set; }

    [ForeignKey("ReportsToEmployeeId")]
    [JsonIgnore]
    public Employee? ReportsTo { get; set; }

    [JsonIgnore]
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

    // Navigation properties
    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [JsonIgnore]
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    [JsonIgnore]
    public LeaveBalance? LeaveBalance { get; set; }
}
