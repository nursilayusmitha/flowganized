using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flowganized.Models;

public class Page
{
    [Key]
    public int PageId { get; set; }

    [ForeignKey("Organization")]
    public int OrganizationId { get; set; }

    [ForeignKey("Department")]
    public int? DepartmentId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    public string? Group { get; set; }

    [Required]
    public string Routes { get; set; } = null!;

    public string? Icon { get; set; }

    public List<PageProp>? Props { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


public class PageProp
{
    public string? IconWidth { get; set; }
    public string? IconHeight { get; set; }
    public string? IconViewBox { get; set; }
}
