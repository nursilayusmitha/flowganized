using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Flowganized.Models;

public class UserOrganization
{
    [Key]
    public int UserOrganizationId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [Required]
    public List<UserPosition> Position { get; set; } = new();

    public string? Avatar { get; set; }

    public string? DisplayName { get; set; }

    [Required]
    public MembershipStatus Status { get; set; }

    [Required]
    public JoinType JoinType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum MembershipStatus
{
    Active,
    Left,
    Kicked
}

public enum JoinType
{
    Created,
    Public,
    Private,
    Token
}

public class UserPosition
{
    public int DepartmentId { get; set; }
    public int RoleId { get; set; }
}
