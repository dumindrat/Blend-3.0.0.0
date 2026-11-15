using System.Data;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Components;
using Blend.BlazorApp.Services;
using Blend.BusinessService.Brokers;
using Blend.DataService.Brokers;
using Blend.DataService.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
AppDomain.CurrentDomain.SetData("DataDirectory", builder.Environment.ContentRootPath);
var identityConnectionString = builder.Configuration.GetConnectionString("BlendIdentity");
if (string.IsNullOrWhiteSpace(identityConnectionString))
{
    throw new InvalidOperationException("Configure ConnectionStrings:BlendIdentity before starting Blend.BlazorApp.");
}

builder.Services.AddDbContextFactory<BlendIdentityDbContext>(options => options.UseSqlServer(identityConnectionString));
builder.Services.AddHttpContextAccessor();
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
builder.Services.AddScoped<BlendUserClaimsPrincipalFactory>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<BlendIdentityUser>>(
    services => services.GetRequiredService<BlendUserClaimsPrincipalFactory>());
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
builder.Services.AddScoped<IBrokerService, BrokerService>();
builder.Services.AddScoped<IBrokerDataService, BrokerDataService>();
builder.Services.AddScoped<IStoredProcedureMapper>(services =>
{
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("BlendData");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Configure ConnectionStrings:BlendData before using broker data services.");
    }

    return new DapperStoredProcedureMapper(() => new SqlConnection(connectionString));
});

var app = builder.Build();

app.UseExceptionHandler("/Error", createScopeForErrors: true);
if (!app.Environment.IsDevelopment())
{
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
