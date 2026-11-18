namespace Blend.BusinessService.Contracts;

public sealed class BuyerContractDetail
{
    public long ContractId { get; set; }
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public decimal OrderQty { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}
