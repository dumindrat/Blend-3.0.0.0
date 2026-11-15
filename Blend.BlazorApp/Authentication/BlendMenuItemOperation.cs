using System.ComponentModel.DataAnnotations;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendMenuItemOperation
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public BlendMenuItem MenuItem { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
