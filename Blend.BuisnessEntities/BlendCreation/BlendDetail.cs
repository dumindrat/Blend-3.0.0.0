using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class BlendDetail : BaseEntity
    {
        public long BlendDetailId { get; set; }
        public long Id { get; set; }
        public long StockItemId { get; set; }
        public string ItemType { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string SaleType { get; set; }
        public string GardenMark { get; set; }
        public string SaleCode { get; set; }
        public string SaleDate { get; set; }

        public decimal NetWeight { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
    }

    public class BlendDetailDto
    {
        public IList<BlendDetail> BlendItem { get; set; }
        public string BlendNo { get; set; }
        public string DisplayRate { get; set; }
        public string DisplayQty { get; set; }

    }
}
