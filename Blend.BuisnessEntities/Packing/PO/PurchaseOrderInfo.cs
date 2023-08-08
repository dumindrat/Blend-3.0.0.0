using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.PO
{
    public class PurchaseOrderInfo : BaseEntity
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public int SupplierId { get; set; }
        public bool IsAutoDocNo { get; set; }
        public int ContractId { get; set; }
        public string DeliveryDate { get; set; }
        public string Instructions { get; set; }
    }
}
