using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class Organization
{
    [Key]
    public int OrganizationId { get; set; }

    [Required, MaxLength(150)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [ForeignKey("Owner")]
    public int OwnerOrganizationId { get; set; }
    public User Owner { get; set; } = null!;

    [Required]
    public OrganizationType OrganizationType { get; set; }

    [Required]
    public OrganizationStatus Status { get; set; }

    public bool AllowMultiDepartment { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum OrganizationType
{
    General,
    Business
}

public enum OrganizationStatus
{
    Active,
    Inactive
}
