using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Flowganized.Models;

public class ResetToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; } = false;

    [Required]
    public virtual User User { get; set; } = null!;
}
