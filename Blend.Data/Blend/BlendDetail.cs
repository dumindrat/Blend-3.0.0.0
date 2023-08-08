namespace Blend.Data.Blend
{
    public class BlendDetail : BaseEntity
    {
        public long BlendId { get; set; }
        public long StockId { get; set; }
        public long ReferenceId { get; set; }
        public string ReferenceNo { get; set; }
        public decimal UsedBags { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
    }
}
