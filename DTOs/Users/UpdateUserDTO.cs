namespace Flowganized.DTOs.Users;

public class UpdateUserDTO
{
    public required string Name { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? ImageName { get; set; }
    public List<string>? Organization { get; set; }
}
