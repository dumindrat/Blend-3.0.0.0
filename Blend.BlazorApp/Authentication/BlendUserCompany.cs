namespace Blend.BlazorApp.Authentication;

public sealed class BlendUserCompany
{
    public string UserId { get; set; } = string.Empty;
    public BlendIdentityUser User { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public BlendCompany Company { get; set; } = null!;
}
