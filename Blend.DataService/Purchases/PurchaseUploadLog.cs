namespace Blend.DataService.Purchases;

public sealed class PurchaseUploadLog
{
    public string? FileName { get; set; }
    public int SaleId { get; set; }
    public int ErrorRecords { get; set; }
    public int TotalRecords { get; set; }
    public int BrokerId { get; set; }
    public string? SaleCode { get; set; }
    public string? SaleType { get; set; }
    public string? UploadData { get; set; }
    public string? CreateUser { get; set; }
}
