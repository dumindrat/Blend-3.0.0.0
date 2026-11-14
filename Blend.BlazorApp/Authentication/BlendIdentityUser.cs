using Microsoft.AspNetCore.Identity;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public Guid? CompanyId { get; set; }
    public BlendCompany? Company { get; set; }
}
