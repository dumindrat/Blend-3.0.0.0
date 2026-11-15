namespace Blend.BlazorApp.Authentication;

public sealed class BlendRoleOperation
{
    public string RoleId { get; set; } = string.Empty;
    public BlendIdentityRole Role { get; set; } = null!;

    public int OperationId { get; set; }
    public BlendMenuItemOperation Operation { get; set; } = null!;
}
