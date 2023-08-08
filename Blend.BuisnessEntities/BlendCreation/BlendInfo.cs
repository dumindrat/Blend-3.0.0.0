using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class BlendInfo : BaseEntity
    {
        public int Id { get; set; }
        public int BlendId { get; set; }
        public string BlendNo { get; set; }
        public bool IsAutoDocNo { get; set; }
        public int ContractId { get; set; }
        public string ContractNo { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCancelRequest { get; set; }
        public decimal ExportQty { get; set; }
        public decimal LineQty { get; set; }
        public decimal Rate { get; set; }
        public string CloseStatus { get; set; }
        public string CompleteDate { get; set; }
        public bool IsTeaBoardEnabled { get; set; }
    }

    public class BlendHeader
    {
        public string BlendNo { get; set; }
        public decimal ExportQty { get; set; }

    }

    public class BlendQuickSearchItem
    {
        public string ItemName { get; set; }
    }
}
