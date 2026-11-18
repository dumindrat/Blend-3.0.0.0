namespace Blend.DataService.Contracts;

public sealed class ContractDetailRecord
{
    public long ContractId { get; set; }
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public decimal OrderQty { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}
