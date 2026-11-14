using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityRole : IdentityRole
{
    public BlendIdentityRole()
    {
    }

    public BlendIdentityRole(string name, string displayName, Guid? companyId)
        : base(name)
    {
        DisplayName = displayName;
        CompanyId = companyId;
    }

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }
    public BlendCompany? Company { get; set; }
}
