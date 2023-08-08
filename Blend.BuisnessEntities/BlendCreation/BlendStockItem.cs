using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class BlendStockItem : BaseEntity
    {
        public int Id { get; set; }
        public int StockItemId { get; set; }
        public string ItemName { get; set; }
        public string StockName { get; set; }
        public string ItemType { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
        public int IsClosed { get; set; }
        public string Status { get; set; }

    }
}
