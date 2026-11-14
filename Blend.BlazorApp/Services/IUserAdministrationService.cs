namespace Blend.BlazorApp.Services;

public interface IUserAdministrationService
{
    Task<UserAdministrationResult> CreateUserAsync(
        string userName,
        string displayName,
        string? email,
        string role,
        CancellationToken cancellationToken = default);
}
