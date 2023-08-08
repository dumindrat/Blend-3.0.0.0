namespace Blend.Data.Blend
{
    public class BlendInfo : BaseEntity
    {
        public string BlendNumber { get; set; }
        public int ContractDetailId { get; set; }
        public decimal ExportQty { get; set; }
        public decimal BlendQty { get; set; }
        public bool IsClosed { get; set; }
        public string ClosedDate { get; set; }
    }
}
