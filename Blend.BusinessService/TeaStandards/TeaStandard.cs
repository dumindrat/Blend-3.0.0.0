namespace Blend.BusinessService.TeaStandards;

public sealed class TeaStandard
{
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}
