using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Approvals.Adjustments
{
    public class AdjustmentCancelReq
    {
        public int Id { get; set; }
        public string BoxNo { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty   { get; set; }
        public int AdjustmentId { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestBy { get; set; }
        public string Cancelreason { get; set; }
        public string AdjustReason { get; set; }
        public string IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public string RowVersion { get; set; }
    }
}
