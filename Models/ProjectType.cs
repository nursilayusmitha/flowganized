using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class ProjectType
{
    [Key]
    public int ProjectTypeId { get; set; }

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
