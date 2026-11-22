using System.Collections.Generic;

namespace Blend.DataService.Reports.Purchase;

public interface IPurchaseReportDataService
{
    IReadOnlyList<SaleSummaryRecord> GetSaleSummary(int saleId);
    IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheet(int saleId);
    IReadOnlyList<CanisterSheetRecord> GetCanisterSheet(int saleId);
    IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByAdvanceFilter(string? fromSaleDate, string? toSaleDate, string? grade, string? standard, string? saleType, string? brokerCode);
    IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByBroker(string? fromSaleDate, string? toSaleDate, int brokerId);
}
