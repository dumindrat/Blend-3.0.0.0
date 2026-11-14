using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public static class BlendIdentityInitializer
{
    private static readonly Guid DefaultCompanyId = new("F0C1A28D-7F0D-43A1-9CE8-9C3A6B63F274");

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<BlendIdentityDbContext>();
        await database.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<BlendIdentityRole>>();
        var defaultCompany = await database.Companies.SingleOrDefaultAsync(company => company.Id == DefaultCompanyId)
            ?? throw new InvalidOperationException("The migrated default company was not found.");
        var platformAdminRole = await roleManager.FindByNameAsync(BlendRoles.Admin);
        if (platformAdminRole is null)
        {
            var result = await roleManager.CreateAsync(new BlendIdentityRole(BlendRoles.Admin, BlendRoles.Admin, companyId: null));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not initialize the platform administrator role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        if (!await roleManager.Roles.AnyAsync(role => role.CompanyId == defaultCompany.Id && role.DisplayName == BlendRoles.User))
        {
            var result = await roleManager.CreateAsync(new BlendIdentityRole($"{defaultCompany.Id:N}:{Guid.NewGuid():N}", BlendRoles.User, defaultCompany.Id));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not initialize the default company user role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        if (!await roleManager.Roles.AnyAsync(role => role.CompanyId == defaultCompany.Id && role.DisplayName == "Company Admin"))
        {
            var companyAdminRole = new BlendIdentityRole($"{defaultCompany.Id:N}:{Guid.NewGuid():N}", "Company Admin", defaultCompany.Id);
            var result = await roleManager.CreateAsync(companyAdminRole);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not initialize the default company admin role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }

            foreach (var permission in new[] { BlendPermissions.ManageUsers, BlendPermissions.ManageRoles, BlendPermissions.ManageMenuItems })
            {
                var claimResult = await roleManager.AddClaimAsync(companyAdminRole, new System.Security.Claims.Claim(BlendPermissions.ClaimType, permission));
                if (!claimResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not initialize the default company admin permissions: {string.Join("; ", claimResult.Errors.Select(error => error.Description))}");
                }
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BlendIdentityUser>>();
        var adminUserName = configuration["Authentication:BootstrapAdmin:UserName"];
        var adminPassword = configuration["Authentication:BootstrapAdmin:Password"];
        var adminDisplayName = configuration["Authentication:BootstrapAdmin:DisplayName"];

        if (string.IsNullOrWhiteSpace(adminUserName) || string.IsNullOrEmpty(adminPassword))
        {
            if ((await userManager.GetUsersInRoleAsync(BlendRoles.Admin)).Count == 0)
            {
                throw new InvalidOperationException(
                    "No administrator exists. Set Authentication:BootstrapAdmin:UserName and Authentication:BootstrapAdmin:Password for first startup.");
            }

            return;
        }

        var admin = await userManager.FindByNameAsync(adminUserName.Trim());
        if (admin is null)
        {
            admin = new BlendIdentityUser
            {
                UserName = adminUserName.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(adminDisplayName) ? adminUserName.Trim() : adminDisplayName.Trim(),
                CompanyId = null,
                MustChangePassword = true
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create the bootstrap administrator: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, BlendRoles.Admin))
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, BlendRoles.Admin);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the bootstrap administrator role: {string.Join("; ", addRoleResult.Errors.Select(error => error.Description))}");
            }
        }
    }
}
