using System;
namespace Blend.Data.Order
{
    public class BuyerContract : BaseEntity
    {
        public long ContractId { get; set; }
        public string ContractName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public DateTime CompletionDate { get; set; }
        public string IsClosed { get; set; }
    }

    public class BuyerContractDetail : BaseEntity
    {
        public long ContractId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; }
        public decimal ExportQty { get; set; }
    }
}
