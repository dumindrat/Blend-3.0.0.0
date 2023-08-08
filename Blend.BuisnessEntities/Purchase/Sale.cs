using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Purchase
{
    public class Sale : BaseEntity
    {
        public int SaleId { get; set; }
        public string SaleDate { get; set; }
        public bool IsClosed { get; set; }
        public string SaleStatus { get; set; }

    }
}
