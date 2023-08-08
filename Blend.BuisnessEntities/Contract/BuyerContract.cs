using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Contract
{
    public class BuyerContract : BaseEntity
    {
        public long ContractId { get; set; }
        public string ContractName { get; set; }
        public string Contract { get; set; }
        public int BuyerId { get; set; }
        public string BuyerCode { get; set; }
        public string BuyerName { get; set; }
        public string CompletionDate { get; set; }
        public bool IsClosed { get; set; }
        public string CloseStatus { get; set; }
    }

    public class BuyerContractDetail : BaseEntity
    {
        public long ContractId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; }
        public decimal OrderQty { get; set; }
    }

    public class BlendNoByCon
    {
        public string BlendNo { get; set; }
        public string ClosedStatus { get; set; }
    }
}
