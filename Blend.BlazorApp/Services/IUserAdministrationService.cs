namespace Blend.BlazorApp.Services;

public interface IUserAdministrationService
{
    Task<UserAdministrationResult> CreateUserAsync(
        string userName,
        string displayName,
        string? email,
        string roleId,
        Guid companyId,
        CancellationToken cancellationToken = default);
}
