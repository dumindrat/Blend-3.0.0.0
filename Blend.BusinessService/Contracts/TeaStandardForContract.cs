namespace Blend.BusinessService.Contracts;

public sealed class TeaStandardForContract
{
    public int ContractId { get; set; }
    public int StandardId { get; set; }
    public string? StandardName { get; set; }
    public string? OrderQty { get; set; }
}
