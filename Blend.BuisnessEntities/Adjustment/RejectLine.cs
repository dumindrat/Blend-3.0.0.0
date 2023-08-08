
namespace Blend.BuisnessEntities.Adjustment
{
    public class AdjutLineDto : BaseEntity
    {
        public long PurchaseId { get; set; }
        public long StockId { get; set; }
        public decimal AdjustBags { get; set; }
        public decimal AdjustQty { get; set; }
        public string AdjustmentType { get; set; }
        public string Reason { get; set; }
    }

    public class AdjutedLineDto
    {
        public int AdjustmentId { get; set; }
        public long PurchaseId { get; set; }
        public long StockItemId { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public string AdjustmentType { get; set; }
        public string Reason { get; set; }
        public byte[] RowVersion { get; set; }
        public string Status { get; set; }
        public string User { get; set; }
        public string ItemName { get; set; }
        public int IsSelected { get; set; }
        public int StatusId { get; set; }
    }

    public class PrintNoteDto
    {
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public long AdjustmentId { get; set; }
        public string ItemName { get; set; }
        public bool IsSelected { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string SaleDate { get; set; }
        public string BrokerCode { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string GardenMark { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }

    }

    public class AdjutLineCancelRequest : BaseEntity
    {
        public long Id { get; set; }
        public int AdjustmentId { get; set; }
        public long StockId { get; set; }
        public string BoxNo { get; set; }
        public string SaleCode { get; set; }
        public string SaleDate { get; set; }
        public string BrokerCode { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string GardenMark { get; set; }
        public string RequestDate { get; set; }
        public string RequestBy { get; set; }
        public string CancelReason { get; set; }
        public string AdjustReason { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
    }
}
