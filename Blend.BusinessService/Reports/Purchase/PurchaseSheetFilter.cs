namespace Blend.BusinessService.Reports.Purchase;

public sealed class PurchaseSheetFilter
{
    public string? FromSaleDate { get; set; }
    public string? ToSaleDate { get; set; }
    public string? Grade { get; set; }
    public string? Standard { get; set; }
    public string? SaleType { get; set; }
    public string? BrokerCode { get; set; }
}
