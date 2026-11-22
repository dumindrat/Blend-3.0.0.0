using Blend.DataService.Reports.Purchase;

namespace Blend.BusinessService.Reports.Purchase;

public sealed class PurchaseReportService : IPurchaseReportService
{
    private readonly IPurchaseReportDataService _dataService;

    public PurchaseReportService(IPurchaseReportDataService dataService) =>
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public IReadOnlyList<SaleSummary> GetSaleSummary(int saleId)
    {
        ValidateSaleId(saleId);
        return _dataService.GetSaleSummary(saleId).Select(record => new SaleSummary
        {
            NoOfLots = record.NoOfLots, BrokerId = record.BrokerId, SaleDate = record.SaleDate,
            BrokerName = record.BrokerName, Weight = record.Weight, Value = record.Value, SaleType = record.SaleType,
            NinetyPercVal = record.NinetyPercVal, TenPercVal = record.TenPercVal, HundredPercVal = record.HundredPercVal
        }).ToArray();
    }

    public IReadOnlyList<PurchaseSheet> GetPurchaseSheet(int saleId)
    {
        ValidateSaleId(saleId);
        return _dataService.GetPurchaseSheet(saleId).Select(ToPurchaseSheet).ToArray();
    }

    public IReadOnlyList<CanisterSheet> GetCanisterSheet(int saleId)
    {
        ValidateSaleId(saleId);
        return _dataService.GetCanisterSheet(saleId).Select(record => new CanisterSheet
        {
            TinNo = record.TinNo, BoxNo = record.BoxNo, LotNo = record.LotNo, Invoice = record.Invoice,
            Grade = record.Grade, Standard = record.Standard, GardenMark = record.GardenMark, SaleDate = record.SaleDate,
            SaleType = record.SaleType, BrokerName = record.BrokerName, Rate = record.Rate, LineQty = record.LineQty,
            RepeatedLineQty = record.RepeatedLineQty, Qty = record.Qty
        }).ToArray();
    }

    public IReadOnlyList<PurchaseSheet> GetPurchaseSheetByAdvanceFilter(PurchaseSheetFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return _dataService.GetPurchaseSheetByAdvanceFilter(filter.FromSaleDate, filter.ToSaleDate, filter.Grade, filter.Standard, filter.SaleType, filter.BrokerCode)
            .Select(ToPurchaseSheet).ToArray();
    }

    public IReadOnlyList<PurchaseSheet> GetPurchaseSheetByBroker(BrokerPurchaseFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return _dataService.GetPurchaseSheetByBroker(filter.FromSaleDate, filter.ToSaleDate, filter.BrokerId)
            .Select(ToPurchaseSheet).ToArray();
    }

    private static void ValidateSaleId(int saleId)
    {
        if (saleId <= 0) throw new ArgumentOutOfRangeException(nameof(saleId));
    }

    private static PurchaseSheet ToPurchaseSheet(PurchaseSheetRecord record) => new()
    {
        BoxNo = record.BoxNo, TinNo = record.TinNo, LotNo = record.LotNo, Invoice = record.Invoice,
        Grade = record.Grade, Standard = record.Standard, GardenMark = record.GardenMark, SaleDate = record.SaleDate,
        SaleType = record.SaleType, SaleCode = record.SaleCode, BrokerName = record.BrokerName,
        NetWeight = record.NetWeight, Packs = record.Packs, Weight = record.Weight, Rate = record.Rate, Value = record.Value,
        IsRepeated = record.IsRepeated, VirtualValue = record.VirtualValue, VirtualWeight = record.VirtualWeight,
        TeaCertificates = record.TeaCertificates
    };
}
