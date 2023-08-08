using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class StraightLineStockItem : BaseEntity
    {
        public long Id { get; set; }
        public int StockItemId { get; set; }
        public string BoxNo { get; set; }
        public int? SaleId { get; set; }
        public string SaleCode { get; set; }
        public string SaleDate { get; set; }
        public string Invoice { get; set; }
        public string LotNo { get; set; }
        public string SaleType { get; set; }
        public string BrokerCode { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Packs { get; set; }
        public decimal? Qty { get; set; }
        public decimal? ReducedQty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Value { get; set; }
        public bool IsRepeated { get; set; }
        public decimal ADPacks { get; set; }
        public decimal ADQty { get; set; }
    }
}
