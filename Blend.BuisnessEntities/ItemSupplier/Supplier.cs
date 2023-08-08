using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.ItemSupplier
{
    public class Supplier : BaseEntity
    {
        public int Id { get; set; }
        public string SupplierName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }

    }
}
