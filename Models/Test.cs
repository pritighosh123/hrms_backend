using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models;

[Table("TEST")]
public class Test
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
