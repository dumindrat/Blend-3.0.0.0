using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityDbContext : IdentityDbContext<BlendIdentityUser>
{
    public BlendIdentityDbContext(DbContextOptions<BlendIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlendMenuItem> MenuItems => Set<BlendMenuItem>();
    public DbSet<BlendMenuItemRole> MenuItemRoles => Set<BlendMenuItemRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BlendMenuItemRole>(entity =>
        {
            entity.HasKey(assignment => new { assignment.MenuItemId, assignment.RoleId });
            entity.HasOne(assignment => assignment.MenuItem)
                .WithMany()
                .HasForeignKey(assignment => assignment.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(assignment => assignment.Role)
                .WithMany()
                .HasForeignKey(assignment => assignment.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
