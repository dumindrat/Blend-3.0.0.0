using Microsoft.AspNetCore.Identity;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendMenuItemRole
{
    public int MenuItemId { get; set; }
    public BlendMenuItem MenuItem { get; set; } = null!;
    public string RoleId { get; set; } = string.Empty;
    public BlendIdentityRole Role { get; set; } = null!;
}
