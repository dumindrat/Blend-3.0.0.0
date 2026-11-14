using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendCompanyAccessMiddleware
{
    private readonly RequestDelegate _next;

    public BlendCompanyAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, BlendIdentityDbContext database)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true || user.IsInRole(BlendRoles.Admin))
        {
            await _next(context);
            return;
        }

        if (IsAllowedWhenCompanyInactive(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var companyIdValue = user.FindFirst(BlendClaims.CompanyId)?.Value;
        if (!Guid.TryParse(companyIdValue, out var companyId))
        {
            context.Response.Redirect("/Account/CompanyAccessUnavailable");
            return;
        }

        var company = await database.Companies
            .AsNoTracking()
            .Where(item => item.Id == companyId)
            .Select(item => new { item.IsEnabled, item.SubscriptionStartDate, item.SubscriptionEndDate })
            .SingleOrDefaultAsync(context.RequestAborted);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isActive = company is not null
            && company.IsEnabled
            && today >= company.SubscriptionStartDate
            && today <= company.SubscriptionEndDate;

        if (isActive)
        {
            await _next(context);
            return;
        }

        context.Response.Redirect("/Account/CompanyAccessUnavailable");
    }

    private static bool IsAllowedWhenCompanyInactive(PathString path) =>
        path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/CompanyAccessUnavailable", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase);
}
