namespace Blend.BusinessService.Reports.Purchase;

public sealed class SaleSummary
{
    public int NoOfLots { get; set; }
    public int BrokerId { get; set; }
    public string? SaleDate { get; set; }
    public string? BrokerName { get; set; }
    public decimal Weight { get; set; }
    public decimal Value { get; set; }
    public string? SaleType { get; set; }
    public decimal NinetyPercVal { get; set; }
    public decimal TenPercVal { get; set; }
    public decimal HundredPercVal { get; set; }
}
