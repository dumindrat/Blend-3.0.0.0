# Blend Blazor Web App

The .NET 10 server-interactive Blazor app uses ASP.NET Core Identity for local account sign-in, cookie authentication, hashed passwords, lockout after repeated failed attempts, and role-based authorization. Identity data is stored in SQL Server through Entity Framework Core; the legacy user and authentication modules are not used by this login flow.

## Configure the Identity database and first administrator

Development uses SQL Server LocalDB and attaches `Blend.BlazorApp\Data\BlendIdentity.mdf`; the app applies EF Core migrations at startup. Install SQL Server Express LocalDB before running the app. For the first startup, provide a bootstrap administrator with .NET User Secrets:

```powershell
dotnet user-secrets set "Authentication:BootstrapAdmin:UserName" "admin" --project Blend.BlazorApp\Blend.BlazorApp.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:Password" "Use-a-unique-secret-of-at-least-12-characters!" --project Blend.BlazorApp\Blend.BlazorApp.csproj
dotnet user-secrets set "Authentication:BootstrapAdmin:DisplayName" "Application administrator" --project Blend.BlazorApp\Blend.BlazorApp.csproj
dotnet run --project Blend.BlazorApp\Blend.BlazorApp.csproj
```

To use a different SQL Server, override `ConnectionStrings:BlendIdentity` in User Secrets with its connection string. User Secrets are for local development, not encrypted, and stored outside the repository. Use a secret store for deployed environments; do not commit real passwords or connection credentials. The bootstrap admin must change the supplied password on first sign-in. After verifying the admin account exists, remove the bootstrap username/password settings with `dotnet user-secrets remove`, specifying each key; subsequent accounts are created by an authenticated Admin at `/Admin/Users`.

The platform `Admin` manages companies at `/Admin/Companies`; company records include contact/address details, an enabled switch, and inclusive UTC subscription start/end dates. The platform Admin role remains global. Regular users and all tenant roles and menu items belong to exactly one company, while usernames remain globally unique. Existing data is migrated into a `Default Company`; existing platform Admin users remain global. A tenant user's login is denied when the company is disabled or its subscription is not active, and already signed-in users are blocked on subsequent requests as well.

Roles and permissions are managed at `/Admin/Roles`. Platform Admins can select a company; a user with `Manage roles` can manage only their own company's roles. The built-in tenant `User` role cannot be renamed or changed. Role permissions include `Manage users`, `Manage roles`, and `Manage menu items`; company administrators receive all three by default. Users with `Manage users` can create accounts only within their company, and role selection is restricted to that company. The platform Admin can select the target company on `/Admin/Users` and `/Admin/MenuItems`. Menu items are company-scoped, assigned from the role page, and displayed in the signed-in user's left navigation by group. Direct requests to configured operation paths are also checked against same-company role assignments; platform Admins see all entries. Operation paths must be local application paths. New users receive a cryptographically generated temporary password, shown only in the creation response, and must change it at first sign-in. After a role permission update, affected users must sign out and back in for the changed permission to take effect. Login uses generic credential errors and locks an account temporarily after repeated failed attempts. Account forms use antiforgery validation and secure password hashing through Identity.

## Data migration status

The new login intentionally replaces the legacy app login. Existing legacy API data operations are not connected to these accounts yet, as requested; the home page indicates that data operations will return after the API/data layer is migrated to this authentication system. Existing .NET Framework 4.8 business/data projects and legacy API authentication remain unchanged.
