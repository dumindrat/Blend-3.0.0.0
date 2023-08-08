using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Shipments
{
    public class TeaShipmentDto : BaseEntity
    {
        public int ShipmetId { get; set; }
        public int BlendId { get; set; }
        public decimal Qty { get; set; }
        public decimal Packs { get; set; }
        public string ExportType { get; set; }
    }

    public class ClosedBlend
    {
        public int BlendId { get; set; }
        public string BlendNo { get; set; }
        public decimal LineQty { get; set; }
        public string ContractName { get; set; }
        public string StandardName { get; set; }
        public string ClosedDate { get; set; }
        public decimal ClosedRate { get; set; }
        public decimal ExportQty { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal STDBB { get; set; }
        public string ClosedUser { get; set; }
        public decimal TotShipQty { get; set; }
        public decimal AvailableBalance { get; set; }
        public string DateOfCompletion { get; set; }
        public byte[] RowVersion { get; set; }
        public bool IsPackGoodsCreate { get; set; }
    }
}
