using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRMS.API.Models;

[Table("Users")]
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=";

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Employee"; // e.g., "Admin", "HR", "Employee"

    public bool MustChangePassword { get; set; } = true;
    public int? EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public Employee? Employee { get; set; }
}
