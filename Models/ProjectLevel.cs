using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class ProjectLevel
{
    [Key]
    public int ProjectLevelId { get; set; }

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
