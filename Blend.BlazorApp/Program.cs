using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components;
using Blend.BlazorApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
AppDomain.CurrentDomain.SetData("DataDirectory", builder.Environment.ContentRootPath);
var identityConnectionString = builder.Configuration.GetConnectionString("BlendIdentity");
if (string.IsNullOrWhiteSpace(identityConnectionString))
{
    throw new InvalidOperationException("Configure ConnectionStrings:BlendIdentity before starting Blend.BlazorApp.");
}

builder.Services.AddDbContext<BlendIdentityDbContext>(options => options.UseSqlServer(identityConnectionString));
builder.Services.AddIdentityCore<BlendIdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<BlendIdentityRole>()
    .AddEntityFrameworkStores<BlendIdentityDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<BlendIdentityUser>, BlendUserClaimsPrincipalFactory>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/";
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PasswordChangeComplete", policy =>
        policy.RequireClaim("blend:must-change-password", "false"));
    options.AddPolicy(BlendPermissions.ManageUsersPolicy, policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole(BlendRoles.Admin)
            || context.User.HasClaim(BlendPermissions.ClaimType, BlendPermissions.ManageUsers)));
    options.AddPolicy(BlendPermissions.ManageRolesPolicy, policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole(BlendRoles.Admin)
            || context.User.HasClaim(BlendPermissions.ClaimType, BlendPermissions.ManageRoles)));
    options.AddPolicy(BlendPermissions.ManageMenuItemsPolicy, policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole(BlendRoles.Admin)
            || context.User.HasClaim(BlendPermissions.ClaimType, BlendPermissions.ManageMenuItems)));
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<IUserAdministrationService, UserAdministrationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<BlendCompanyAccessMiddleware>();
app.UseMiddleware<BlendMenuItemAuthorizationMiddleware>();
app.UseAuthorization();
app.UseAntiforgery();

await BlendIdentityInitializer.InitializeAsync(app.Services, app.Configuration);

app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
