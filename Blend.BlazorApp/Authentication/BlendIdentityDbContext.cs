using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityDbContext : IdentityDbContext<BlendIdentityUser>
{
    public BlendIdentityDbContext(DbContextOptions<BlendIdentityDbContext> options)
        : base(options)
    {
    }
}
