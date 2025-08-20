using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Flowganized.Models;

public class EmailVerificationToken
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; } = false;


    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
