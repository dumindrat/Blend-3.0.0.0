using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Item
{
    public class PoItem : ItemMaster
    {
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public string Comment { get; set; }
        public bool IsAdded { get; set; }
    }
}
