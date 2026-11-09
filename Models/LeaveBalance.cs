using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRMS.API.Models;

[Table("LeaveBalances")]
public class LeaveBalance
{
    [Key]
    public int BalanceId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public Employee? Employee { get; set; }

    public int TotalLeaves { get; set; } = 24;

    public int UsedLeaves { get; set; } = 0;

    [NotMapped]
    public int RemainingLeaves => TotalLeaves - UsedLeaves;
}
