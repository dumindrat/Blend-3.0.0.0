using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Reports.Purchase;

public sealed class PurchaseReportDataService : IPurchaseReportDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public PurchaseReportDataService(IStoredProcedureMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public IReadOnlyList<SaleSummaryRecord> GetSaleSummary(int saleId) =>
        _mapper.Query<SaleSummaryRecord>("rpt_SalesSummary", new { SaleId = saleId });

    public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheet(int saleId) =>
        _mapper.Query<PurchaseSheetRecord>("rpt_PurchaseSheetBySaleId", new { SaleId = saleId });

    public IReadOnlyList<CanisterSheetRecord> GetCanisterSheet(int saleId) =>
        _mapper.Query<CanisterSheetRecord>("rpt_CanisterSheetBySaleId", new { SaleId = saleId });

    public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByAdvanceFilter(string? fromSaleDate, string? toSaleDate, string? grade, string? standard, string? saleType, string? brokerCode) =>
        _mapper.Query<PurchaseSheetRecord>("rpt_PurchaseSheetBySaleDate", new
        {
            FromSaleDate = fromSaleDate, ToSaleDate = toSaleDate, Standard = standard,
            Grade = grade, SaleType = saleType, BrokerCode = brokerCode
        });

    public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByBroker(string? fromSaleDate, string? toSaleDate, int brokerId) =>
        _mapper.Query<PurchaseSheetRecord>("rpt_PurchaseSheetByBroker", new { FromSaleDate = fromSaleDate, ToSaleDate = toSaleDate, BrokerId = brokerId });
}
