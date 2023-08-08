using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Report
{
    public class PurchaseSheetRptDto
    {
        public int SaleId { get; set; }
        public bool IsWithValues { get; set; }
        public bool IsExcelFormat { get; set; }
        public bool IsDownload { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string FileType { get; set; }


    }

    public class PurchaseSheetAdvanceRptDto
    {
        public string FromSaleDate { get; set; }
        public string ToSaleDate { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string SaleType { get; set; }
        public string BrokerCode { get; set; }
        public string GardenMark { get; set; }
        public bool IsExcelFormat { get; set; }
        public bool IsWithValues { get; set; }
    }

    public class PurchaseSheetParam
    {
        public string FromSaleDate { get; set; }
        public string ToSaleDate { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string SaleType { get; set; }
        public string BrokerCode { get; set; }
        public int BrokerId { get; set; }
    }

    public class BlendCanister
    {
        public string BlendNo { get; set; }
        public string StockType { get; set; }
        public string Standard { get; set; }
        public string StockQty { get; set; }
        public string Rate { get; set; }
    }

    public class BrokerPurchaeRptDto
    {
        public string FromSaleDate { get; set; }
        public string ToSaleDate { get; set; }
        public int BrokerId { get; set; }
        public string BrokerName { get; set; }
        public bool IsExcelFormat { get; set; }
        public bool IsWithValues { get; set; }
    }
}
