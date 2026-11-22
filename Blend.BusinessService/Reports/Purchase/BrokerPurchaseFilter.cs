namespace Blend.BusinessService.Reports.Purchase;

public sealed class BrokerPurchaseFilter
{
    public string? FromSaleDate { get; set; }
    public string? ToSaleDate { get; set; }
    public int BrokerId { get; set; }
}
