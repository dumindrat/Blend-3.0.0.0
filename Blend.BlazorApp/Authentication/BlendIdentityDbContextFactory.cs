using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendIdentityDbContextFactory : IDesignTimeDbContextFactory<BlendIdentityDbContext>
{
    public BlendIdentityDbContext CreateDbContext(string[] args)
    {
        var contentRoot = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(contentRoot, "appsettings.json"))
            && File.Exists(Path.Combine(contentRoot, "Blend.BlazorApp", "appsettings.json")))
        {
            contentRoot = Path.Combine(contentRoot, "Blend.BlazorApp");
        }

        AppDomain.CurrentDomain.SetData("DataDirectory", contentRoot);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(contentRoot)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("BlendIdentity");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Set ConnectionStrings:BlendIdentity before creating Identity migrations.");
        }

        var options = new DbContextOptionsBuilder<BlendIdentityDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new BlendIdentityDbContext(options);
    }
}
