namespace Blend.DataService.Purchases;

public sealed class PurchaseSaleRecord
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? SaleDate { get; set; }
    public bool IsClosed { get; set; }
    public string? SaleStatus { get; set; }
    public string? ModifiedDate { get; set; }
    public string? ModifiedUser { get; set; }
    public string? User { get; set; }
}
