using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.Data.Order
{
    public class AddedStandardDto
    {
        public long ContractId { get; set; }
        public long StandardId { get; set; }
        public string Description { get; set; }
        public decimal ExportQty { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
