using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Flowganized.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(150)]
    public required string Name { get; set; }

    [Required, MaxLength(20)]
    public required string Username { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }
    [MinLength(8)]
    public string Password { get; set; } = null!;

    // Sisanya bisa tetap nullable:
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? ImageName { get; set; }
    public List<string>? Organization { get; set; }
    public bool IsVerified { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
