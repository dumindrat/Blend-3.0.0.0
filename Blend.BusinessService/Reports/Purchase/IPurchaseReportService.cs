namespace Blend.BusinessService.Reports.Purchase;

public interface IPurchaseReportService
{
    IReadOnlyList<SaleSummary> GetSaleSummary(int saleId);
    IReadOnlyList<PurchaseSheet> GetPurchaseSheet(int saleId);
    IReadOnlyList<CanisterSheet> GetCanisterSheet(int saleId);
    IReadOnlyList<PurchaseSheet> GetPurchaseSheetByAdvanceFilter(PurchaseSheetFilter filter);
    IReadOnlyList<PurchaseSheet> GetPurchaseSheetByBroker(BrokerPurchaseFilter filter);
}
