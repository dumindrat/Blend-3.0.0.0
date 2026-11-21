namespace Blend.DataService.Purchases;

public sealed class PurchaseLineRecord
{
    public long Id { get; set; }
    public long StockId { get; set; }
    public string? BoxNo { get; set; }
    public int? SaleId { get; set; }
    public string? SaleCode { get; set; }
    public string? Invoice { get; set; }
    public string? LotNo { get; set; }
    public int? BrokerId { get; set; }
    public string? BrokerCode { get; set; }
    public string? SaleType { get; set; }
    public string? Grade { get; set; }
    public string? Standard { get; set; }
    public string? GardenMark { get; set; }
    public string? TeaCertificates { get; set; }
    public decimal NetWeight { get; set; }
    public decimal Packs { get; set; }
    public decimal? Weight { get; set; }
    public decimal? ReducedQty { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Value { get; set; }
    public string? TinNo { get; set; }
    public bool IsRepeated { get; set; }
    public decimal? VirtualWeight { get; set; }
    public decimal? VirtualValue { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}
