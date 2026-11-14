using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendMenuItemAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public BlendMenuItemAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, BlendIdentityDbContext database)
    {
        var requestPath = context.Request.Path.Value;
        if (string.IsNullOrEmpty(requestPath) || IsStaticResource(requestPath))
        {
            await _next(context);
            return;
        }

        var menuItems = await database.MenuItems
            .AsNoTracking()
            .Select(item => new { item.Id, item.Path })
            .ToListAsync(context.RequestAborted);
        var menuItem = menuItems
            .Where(item => PathMatches(requestPath, item.Path))
            .OrderByDescending(item => item.Path.Length)
            .FirstOrDefault();

        if (menuItem is null)
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await context.ChallengeAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        if (context.User.IsInRole(BlendRoles.Admin))
        {
            await _next(context);
            return;
        }

        var allowedRoleNames = await database.MenuItemRoles
            .AsNoTracking()
            .Where(assignment => assignment.MenuItemId == menuItem.Id)
            .Select(assignment => assignment.Role.Name!)
            .ToListAsync(context.RequestAborted);

        if (allowedRoleNames.Any(context.User.IsInRole))
        {
            await _next(context);
            return;
        }

        await context.ForbidAsync(IdentityConstants.ApplicationScheme);
    }

    private static bool PathMatches(string requestPath, string menuPath)
    {
        var normalizedMenuPath = menuPath.TrimEnd('/');
        if (normalizedMenuPath.Length == 0)
        {
            normalizedMenuPath = "/";
        }

        return requestPath.Equals(normalizedMenuPath, StringComparison.OrdinalIgnoreCase)
            || (normalizedMenuPath != "/"
                && requestPath.StartsWith(normalizedMenuPath + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsStaticResource(string path) =>
        path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/_content/", StringComparison.OrdinalIgnoreCase);
}
