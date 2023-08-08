using Blend.BuisnessEntities.Report;
using Blend.BuisnessEntities.Stock;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Reports.Stock
{
    public class StockReportService : BaseManagement<StraightLineStockReportDto>, IStockReportService
    {
        public IList<BuisnessEntities.Stock.StraightLineStockReportDto> GetStraighLineStock(StockRptDto obj)
        {
            if (!obj.IsWithAllocatedLine && !obj.IsAvailableLineOnly && !obj.IsAllocatedLineOnly)
                obj.IsAvailableLineOnly = true;

            var tag = obj.IsWithAllocatedLine == true ? "GetStraightLineStockReport" : null;
            tag = obj.IsAvailableLineOnly == true ? "GetStraightLineStockReportNoAllocation" : tag;
            tag = obj.IsAllocatedLineOnly == true ? "GetStraightLineStockReportAlloctionOnly" : tag;
            var mapper = new DataMapper<StraightLineStockReportDto>(tag, new Hashtable() { { "Grade", obj.Grade }, { "Standard", obj.Standard } });
            var result = mapper.ObjectList();
            return result;
        }


        public IList<BlendBalanceStockDto> GetBlendBalanceStock(bool withAllocation)
        {
            var tag = withAllocation == true ? "GetBlendBalanceStockReport" : "GetBlendBalanceStockReportNoAllocation";
            var mapper = new DataMapper<BlendBalanceStockDto>(tag, new Hashtable() { });
            var result = mapper.ObjectList();
            return result;

        }


        public IList<StraightLineAgeAnalysisReportDto> GetStraighLineAgeAnalysisStock(AgeAnalysisStockRptDto obj)
        {
            var mapper = new DataMapper<StraightLineAgeAnalysisReportDto>("GetAgeAnalysisStockReport",
                new AgeAnalysisParam { AsAtDate = obj.AsAtDate, Grade = obj.Grade, Standard = obj.Standard });
            var result = mapper.ObjectList();
            return result;
        }


        public IList<FullStockReportDto> GetStraighLineAndBlendBalanceStock(StockRptDto obj)
        {
            var mapper = new DataMapper<FullStockReportDto>("GetFullStockReport", new Hashtable() { });
            var result = mapper.ObjectList();
            return result;

        }


        public IList<StoresAnalysisData> GetStoresAnalysis(StoresAnalysis obj)
        {
            var mapper = new DataMapper<StoresAnalysisData>("GetStoresAnalysisReport", new Hashtable() { { "FromDate", obj.FromDate }, { "ToDate", obj.ToDate } });
            var result = mapper.ObjectList();
            return result;
        }
    }
}
