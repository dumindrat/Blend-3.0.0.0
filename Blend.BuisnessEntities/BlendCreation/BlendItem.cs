using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class BlendItem : BaseEntity
    {
        public long BlendDetailId { get; set; }
        public long BlendId { get; set; }
        public long StockId { get; set; }
        public long StockItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public decimal BlendQty { get; set; }
    }
}
