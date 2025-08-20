using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class InviteLink
{
    [Key]
    public int InviteId { get; set; }

    [Required]
    public string Token { get; set; } = null!; // Bisa GUID atau 4-char

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }

    [ForeignKey("Role")]
    public int RoleId { get; set; }

    [Required]
    public InviteType Type { get; set; }

    public int? MaxUseCount { get; set; } = null; // ∞ jika null
    public int CurrentUseCount { get; set; } = 0;

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum InviteType
{
    Public,
    Private,
    Token
}
