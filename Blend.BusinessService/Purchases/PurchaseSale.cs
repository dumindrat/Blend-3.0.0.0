namespace Blend.BusinessService.Purchases;

public sealed class PurchaseSale
{
    public int SaleId { get; set; }
    public int CompanyId { get; set; }
    public string? SaleDate { get; set; }
    public bool IsClosed { get; set; }
    public string? SaleStatus { get; set; }
    public string? ModifiedDate { get; set; }
    public string? ModifiedUser { get; set; }
    public string? User { get; set; }
}
