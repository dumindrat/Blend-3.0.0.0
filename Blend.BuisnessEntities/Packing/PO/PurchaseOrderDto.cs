using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.PO
{
    public class PurchaseOrderDto : BaseEntity
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public int SupplierId { get; set; }
        public string Supplier { get; set; }
        public string SupplierEmail { get; set; }
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string DeliveryDate { get; set; }
        public bool IsVoid { get; set; }
        public bool IClosed { get; set; }
        public string Instructions { get; set; }

    }

    public class PurchaseOrderItemDto : BaseEntity
    {
        public int PoId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ShortDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Value
        {
            get { return Qty * Price; }

        }
        public string Comment { get; set; }
        public string UomName { get; set; }
        public bool IsAdded { get; set; }
    }

    public class PODto
    {
        public decimal Value { get; set; }
        public int Status { get; set; }
        public IEnumerable<PurchaseOrderItemDto> Items { get; set; }
    }
}
