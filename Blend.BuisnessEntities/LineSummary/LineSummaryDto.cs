using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.LineSummary
{
    public class LineSummaryDto
    {
        public int Id { get; set; }
        public string BoxNo { get; set; }
        public string SaleDate { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string BrokerCode { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public decimal Packs { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public decimal ReducedQty { get; set; }
        public decimal Value { get; set; }
        public decimal StockPacks { get; set; }
        public decimal StockQty { get; set; }
        public string TinNo { get; set; }
        public string CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string ModifiedDate { get; set; }
        public string ModifiedUser { get; set; }
        public decimal VirtualWeight { get; set; }
        public decimal VirtualValue { get; set; }
    }

    public class LineAllocationDto
    {
        public string BlendNo { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public string CreateDate { get; set; }
        public string CreateUser { get; set; }
    }

    public class LineAdjustmentDto
    {
        public string BoxNo { get; set; }
        public string TransType { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
    }

    public class LineTransHistory
    {
        public int Id { get; set; }
        public decimal Qty { get; set; }
        public decimal Packs { get; set; }
        public string TransTo { get; set; }
        public string TransType { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
    }
}
