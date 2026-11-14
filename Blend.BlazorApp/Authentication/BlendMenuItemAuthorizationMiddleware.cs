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

        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        var isPlatformAdmin = context.User.IsInRole(BlendRoles.Admin);
        var companyId = Guid.TryParse(context.User.FindFirst(BlendClaims.CompanyId)?.Value, out var parsedCompanyId)
            ? parsedCompanyId
            : (Guid?)null;

        var menuItems = await database.MenuItems
            .AsNoTracking()
            .Where(item => !isAuthenticated || isPlatformAdmin || item.CompanyId == companyId)
            .Select(item => new { item.Id, item.CompanyId, item.Path })
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

        if (!isAuthenticated)
        {
            await context.ChallengeAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        if (isPlatformAdmin)
        {
            await _next(context);
            return;
        }

        if (companyId is null)
        {
            await context.ForbidAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        var roleIds = context.User.FindAll(BlendClaims.RoleId)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var hasAccess = roleIds.Length > 0 && await database.MenuItemRoles
            .AsNoTracking()
            .AnyAsync(assignment => assignment.MenuItemId == menuItem.Id
                && assignment.MenuItem.CompanyId == companyId
                && roleIds.Contains(assignment.RoleId)
                && assignment.Role.CompanyId == companyId,
                context.RequestAborted);

        if (hasAccess)
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
