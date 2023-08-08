using Blend.BuisnessEntities.Report;
using Blend.BuisnessEntities.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Reports.Stock
{
    public interface IStockReportService
    {
        IList<StraightLineStockReportDto> GetStraighLineStock(StockRptDto objj);
        IList<BlendBalanceStockDto> GetBlendBalanceStock(bool withAllocation);
        IList<FullStockReportDto> GetStraighLineAndBlendBalanceStock(StockRptDto obj);
        IList<StraightLineAgeAnalysisReportDto> GetStraighLineAgeAnalysisStock(AgeAnalysisStockRptDto obj);
        IList<StoresAnalysisData> GetStoresAnalysis(StoresAnalysis obj);
    }
}
