using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessEntities.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Reports.Purchase
{
    public interface IPurchaseReportService
    {
        IList<SaleSummaryDto> GetSaleSummary(int saleId);
        IList<PurchaseSheetDto> GetPurchaseSheet(int saleId);
        IList<CanisterSheetDto> GetCanisterSheet(int saleId);
        IList<PurchaseSheetDto> GetPurchaseSheetByAdvanceFilter(PurchaseSheetAdvanceRptDto obj);
        IList<PurchaseSheetDto> GetPurchaseSheetByBroker(BrokerPurchaeRptDto obj);

    }
}
