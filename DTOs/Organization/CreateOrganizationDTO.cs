using System.ComponentModel.DataAnnotations;
using Flowganized.Models;
namespace Flowganized.DTOs.Organization;

public class CreateOrganizationDTO
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public OrganizationType OrganizationType { get; set; }
}
