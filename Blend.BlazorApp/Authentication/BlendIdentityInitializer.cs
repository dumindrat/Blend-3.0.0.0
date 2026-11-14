using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public static class BlendIdentityInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<BlendIdentityDbContext>();
        await database.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { BlendRoles.Admin, BlendRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not initialize the '{role}' role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
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
