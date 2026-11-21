namespace Blend.BusinessService.Purchases;

public sealed class StraightLine
{
    public long Id { get; set; }
    public long RepeatingId { get; set; }
    public long StockId { get; set; }
    public long RepeatingStockId { get; set; }
    public int CompanyId { get; set; }
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
    public decimal Net1 { get; set; }
    public decimal Pack1 { get; set; }
    public decimal Net2 { get; set; }
    public decimal Pack2 { get; set; }
    public byte[]? RowVersion1 { get; set; }
    public byte[]? RowVersion2 { get; set; }
    public byte[]? RowVersion { get; set; }
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
    public string? User { get; set; }
}
