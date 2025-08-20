using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }

    [ForeignKey("DepartmentParent")]
    public int? DepartmentParentId { get; set; }

    public Department? DepartmentParent { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsGeneral { get; set; } = false;

    [Required]
    public DepartmentStatus Status { get; set; } = DepartmentStatus.Active;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum DepartmentStatus
{
    Active,
    Inactive
}
