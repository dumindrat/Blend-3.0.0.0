using System.ComponentModel.DataAnnotations;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendModule
{
    public int Id { get; set; }
    public Guid CompanyId { get; set; }
    public BlendCompany Company { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;
}
