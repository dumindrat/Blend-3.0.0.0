using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendClose
{
    public class BlendCloseDto : BaseEntity
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public long BlendId { get; set; }
        public string BlendNo { get; set; }
        public decimal ExportQty { get; set; }
        public decimal PhysicalQty { get; set; }
        public decimal BB { get; set; }
        public decimal BG { get; set; }
        public decimal BL { get; set; }
        public decimal SiftedQty { get; set; }
        public decimal STDBB { get; set; }
        public decimal STDBG { get; set; }
        public decimal SFTBB { get; set; }
        public decimal SFTBG { get; set; }
        public decimal Rate { get; set; }
        public decimal ClosedRate { get; set; }
        public string CloseDate { get; set; }
        public bool IsConverted { get; set; }
        public bool IsCancelRequest { get; set; }
        public bool IsClosed { get; set; }
        public bool IsShipment { get; set; }
        public bool IsPackGoodsCreate { get; set; }
        public string PackGoodsCreateDate { get; set; }
        public decimal LineQty { get; set; }
    }

    public class BlendCloseReversDto : BaseEntity
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public long BlendId { get; set; }
        public string Reason { get; set; }
    }

    public class BlendCloseCancelRequest
    {
        public int Id { get; set; }
        public int CategorizedId { get; set; }
        public int StockId { get; set; }
        public int StockItemId { get; set; }
        public string BlendNo { get; set; }
        public decimal LineQty { get; set; }
        public decimal ExportQty { get; set; }
        public decimal CreateQty { get; set; }
        public decimal BB { get; set; }
        public decimal BG { get; set; }
        public decimal BL { get; set; }
        public decimal SIFTED { get; set; }
        public decimal STDBB { get; set; }
        public decimal STDBG { get; set; }
        public decimal SFTBB { get; set; }
        public decimal SFTBG { get; set; }
        public decimal Rate { get; set; }
        public string RequestDate { get; set; }
        public string RequestBy { get; set; }
        public string Reason { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class BlendGainLossRateConvert : BaseEntity
    {
        public int BlendId { get; set; }
        public decimal BG { get; set; }
        public decimal BL { get; set; }
        public decimal BB { get; set; }
        public decimal ClosedRate { get; set; }
        public decimal ConvertedRate { get; set; }
    }

}
