using System.ComponentModel.DataAnnotations;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendMenuItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string GroupName { get; set; } = "Operations";

    [Required]
    [StringLength(512)]
    public string Path { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
