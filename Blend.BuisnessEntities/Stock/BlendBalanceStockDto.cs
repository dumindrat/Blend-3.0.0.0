using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Stock
{
    public class BlendBalanceStockDto
    {
        public string AllocatedBlend { get; set; }
        public string StockItemId { get; set; }
        public string ItemName { get; set; }
        public string Standard { get; set; }
        public string Status { get; set; }
        public string ClosedDate { get; set; }
        public string Qty { get; set; }
        public string Rate { get; set; }
        public string Value { get; set; }
    }
}
