using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class Role
{
    [Key]
    public int RoleId { get; set; }

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    [ForeignKey("Hierarchy")]
    public int HierarchyId { get; set; }

    // Simpan list PageId
    public List<int> AccessPage { get; set; } = new();

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
