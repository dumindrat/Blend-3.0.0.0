using Blend.BuisnessEntities.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Packing.ItemIssue
{
    public class ItemIssueNoteDto : BaseEntity
    {
        public int Id { get; set; }
        public string IssueNoteNo { get; set; }
        public string BlendNo { get; set; }
        public int BlendId { get; set; }
        public bool IsAutoDocNo { get; set; }
    }

    public class IssueNoteItem : BaseEntity
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public int IssueNoteId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int UomId { get; set; }
        public string UomName { get; set; }
        public string StoreLocation { get; set; }
        public string ShortDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class StockItem
    {
        public int Id { get; set; }
        public int StockItemId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int UomId { get; set; }
        public string UomName { get; set; }
        public string ShortDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public string ReceivedDate { get; set; }
        public string StoreLocation { get; set; }
    }
}
