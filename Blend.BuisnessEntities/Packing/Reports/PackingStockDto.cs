using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.Reports
{
    public class PackingStockDto : BaseEntity
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public string Qty { get; set; }
        public string ReceivedPrice { get; set; }
        public string Value { get; set; }
        public string StoreLocation { get; set; }
        public string BlendNo { get; set; }
    }
}
