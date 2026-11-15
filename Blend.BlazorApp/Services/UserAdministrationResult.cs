namespace Blend.BlazorApp.Services;

public sealed class UserAdministrationResult
{
    public bool Succeeded { get; init; }
    public bool IsExistingUser { get; init; }
    public string? TemporaryPassword { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
