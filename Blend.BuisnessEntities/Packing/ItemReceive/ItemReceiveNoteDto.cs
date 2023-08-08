using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.ItemReceive
{
    public class ItemReceiveNoteDto : BaseEntity
    {
        public int Id { get; set; }
        public string ReceiveNoteNo { get; set; }
        public int PoId { get; set; }
        public string PoNumber { get; set; }
        public string SupplierName { get; set; }
        public bool IsAutoDocNo { get; set; }
        public string Description
        {
            get { return ReceiveNoteNo + " | " + PoNumber + " | " + SupplierName; }

        }

    }
    public class ReceivedItemDto
    {
        public int ItemReceivedNoteId { get; set; }
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedDate { get; set; }

    }
    public class AvailablePoDto
    {
        public int PoId { get; set; }
        public string PoNumber { get; set; }
        public string Contract { get; set; }
        public string Suplier { get; set; }
        public string Description
        {
            get { return PoNumber + " | " + Suplier; }

        }
    }
    public class ReceiveNoteItems
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ShortDescription { get; set; }
        public decimal OrderQty { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedPrice { get; set; }
        public string Comment { get; set; }
        public string UomName { get; set; }
        public string ReceivedDate { get; set; }
        public string StoreLocation { get; set; }
        public decimal StockQty { get; set; }
    }
    public class UpdateReceivedItemDto : BaseEntity
    {
        public int Id { get; set; }
        public int ReceivedNoteId { get; set; }
        public int ItemId { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedPrice { get; set; }
        public string ReceivedDate { get; set; }
        public string StoreLocation { get; set; }

    }
}
