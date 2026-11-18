namespace Blend.DataService.TeaStandards;

public sealed class TeaStandardRecord
{
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}
