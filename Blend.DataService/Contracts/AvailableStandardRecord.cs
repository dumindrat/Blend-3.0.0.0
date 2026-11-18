namespace Blend.DataService.Contracts;

public sealed class AvailableStandardRecord
{
    public int ContractId { get; set; }
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public string? OrderQty { get; set; }
}
