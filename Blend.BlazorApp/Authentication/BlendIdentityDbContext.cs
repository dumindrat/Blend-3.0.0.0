using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityDbContext : IdentityDbContext<BlendIdentityUser, BlendIdentityRole, string>
{
    public BlendIdentityDbContext(DbContextOptions<BlendIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlendCompany> Companies => Set<BlendCompany>();
    public DbSet<BlendModule> Modules => Set<BlendModule>();
    public DbSet<BlendMenuItem> MenuItems => Set<BlendMenuItem>();
    public DbSet<BlendMenuItemRole> MenuItemRoles => Set<BlendMenuItemRole>();
    public DbSet<BlendMenuItemOperation> MenuItemOperations => Set<BlendMenuItemOperation>();
    public DbSet<BlendRoleOperation> RoleOperations => Set<BlendRoleOperation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BlendIdentityUser>()
            .HasOne(user => user.Company)
            .WithMany()
            .HasForeignKey(user => user.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BlendIdentityRole>()
            .HasOne(role => role.Company)
            .WithMany()
            .HasForeignKey(role => role.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BlendIdentityRole>()
            .HasIndex(role => new { role.CompanyId, role.DisplayName })
            .IsUnique()
            .HasFilter("[CompanyId] IS NOT NULL");

        builder.Entity<BlendCompany>()
            .HasIndex(company => company.CompanyNumber)
            .IsUnique();

        builder.Entity<BlendModule>(entity =>
        {
            entity.HasIndex(module => new { module.CompanyId, module.Name }).IsUnique();
            entity.HasOne(module => module.Company)
                .WithMany()
                .HasForeignKey(module => module.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BlendMenuItem>()
            .HasOne(item => item.Company)
            .WithMany()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BlendMenuItem>()
            .HasOne(item => item.Module)
            .WithMany()
            .HasForeignKey(item => item.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<BlendMenuItemOperation>(entity =>
        {
            entity.HasIndex(operation => new { operation.MenuItemId, operation.Key }).IsUnique();
            entity.HasOne(operation => operation.MenuItem)
                .WithMany(menuItem => menuItem.Operations)
                .HasForeignKey(operation => operation.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<BlendRoleOperation>(entity =>
        {
            entity.HasKey(assignment => new { assignment.RoleId, assignment.OperationId });
            entity.HasOne(assignment => assignment.Role)
                .WithMany()
                .HasForeignKey(assignment => assignment.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(assignment => assignment.Operation)
                .WithMany()
                .HasForeignKey(assignment => assignment.OperationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(assignment => assignment.OperationId);
        });
    }
}
