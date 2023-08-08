using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.ItemReturn
{
    public class ItemReturnNoteDto : BaseEntity
    {
        public int Id { get; set; }
        public string ReturnNoteNo { get; set; }
        public string ReceiveNoteNo { get; set; }
        public int ReceiveNoteId { get; set; }
        public string PoNumber { get; set; }
        public int PoId { get; set; }
        public string Supplier { get; set; }
        public int SupplierId { get; set; }
        public bool IsAutoDocNo { get; set; }
        public string Comment { get; set; }
    }

    public class ReturnNoteItems: BaseEntity
    {
        public int Id { get; set; }
        public int ReturnNoteId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UomName { get; set; }
        public string ShortDescription { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal Price { get; set; }
        public string Reason { get; set; }
        public int StockId { get; set; }
    }

    public class ReceiveNoteStockItems
    {
        public int Id { get; set; }
        public int ReceveNoteId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UomName { get; set; }
        public string ShortDescription { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedPrice { get; set; }
        public decimal StockQty { get; set; }
        public int StockItemId { get; set; }
        public int StockId { get; set; }
    }
}
