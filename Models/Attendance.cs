using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRMS.API.Models;

[Table("Attendance")]
public class Attendance
{
    [Key]
    public int AttendanceId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public Employee? Employee { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present"; // e.g., "Present", "Absent", "HalfDay"
}
