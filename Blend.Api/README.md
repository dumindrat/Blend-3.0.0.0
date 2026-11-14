# Blend.Api

This ASP.NET Core Web API runs as a standalone Kestrel-hosted application, separate from the legacy IIS-hosted `Blend.WebApp` API.

The starter endpoint is `GET /api/sample` and requires a JWT bearer token. Configure the trusted token issuer and audience through application configuration before using it:

- `Authentication:Authority` (environment variable: `Authentication__Authority`)
- `Authentication:Audience` (environment variable: `Authentication__Audience`)

Production startup fails when either setting is missing. In Development, the API can start without these values, but authenticated requests will not succeed until a valid issuer and audience are configured. The API validates tokens issued by that authority; it does not issue tokens.

Run the project with `dotnet run --project Blend.Api\Blend.Api.csproj`. OpenAPI is available at `/openapi/v1.json` in Development.
