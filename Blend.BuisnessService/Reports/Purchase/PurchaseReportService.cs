using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessEntities.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Reports.Purchase
{
    public class PurchaseReportService : BaseManagement<SaleSummaryDto>, IPurchaseReportService
    {
        public IList<BuisnessEntities.Purchase.SaleSummaryDto> GetSaleSummary(int saleId)
        {
            var mapper = new DataMapper<SaleSummaryDto>("GetSalesSummay", new Hashtable() { { "SaleId", saleId } });

            var result = mapper.ObjectList();
            return result;
        }

        public IList<PurchaseSheetDto> GetPurchaseSheet(int saleId)
        {
            var mapper = new DataMapper<PurchaseSheetDto>("GetPurchaseSheet", new Hashtable() { { "SaleId", saleId } });

            var result = mapper.ObjectList();
            return result;
        }


        public IList<CanisterSheetDto> GetCanisterSheet(int saleId)
        {
            var mapper = new DataMapper<CanisterSheetDto>("GetCanisterSheet", new Hashtable() { { "SaleId", saleId } });

            var result = mapper.ObjectList();
            return result;
        }


        public IList<PurchaseSheetDto> GetPurchaseSheetByAdvanceFilter(BuisnessEntities.Report.PurchaseSheetAdvanceRptDto obj)
        {
            //var searchStr = "Where SaleDate between " + obj.FromSaleDate + "and " + obj.ToSaleDate + "and " + "Grade like %" + obj.Grade +
            //    " Standard like %" + obj.Standard + " SaleType like  %" + obj.SaleType + "BrokerCode like %" + obj.BrokerCode;

            var mapper = new DataMapper<PurchaseSheetDto>("GetPurchaseSheetByFilter",
                new PurchaseSheetParam() { FromSaleDate = obj.FromSaleDate, ToSaleDate = obj.ToSaleDate, Grade = obj.Grade, Standard = obj.Standard, SaleType = obj.SaleType, BrokerCode = obj.BrokerCode });

            var result = mapper.ObjectList();
            return result;
        }


        public IList<PurchaseSheetDto> GetPurchaseSheetByBroker(BrokerPurchaeRptDto obj)
        {
            var mapper = new DataMapper<PurchaseSheetDto>("GetPurchaseSheetByBroker",
                new PurchaseSheetParam() { FromSaleDate = obj.FromSaleDate, ToSaleDate = obj.ToSaleDate, BrokerId = obj.BrokerId });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
