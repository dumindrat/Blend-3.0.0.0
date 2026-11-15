namespace Blend.BusinessService;

public sealed record BusinessOperationResult(bool Succeeded, string? ErrorMessage = null)
{
    public static BusinessOperationResult Success() => new(true);

    public static BusinessOperationResult Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new BusinessOperationResult(false, errorMessage);
    }
}
