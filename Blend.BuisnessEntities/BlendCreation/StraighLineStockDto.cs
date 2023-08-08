using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class StraighLineStockDto
    {
        public string BoxNo { get; set; }
        public long Id { get; set; }
        public int StockItemId { get; set; }
        public Decimal Net { get; set; }
        public Decimal Pack { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? ReducedQty { get; set; }
        public byte[] RowVersion { get; set; }

        public long IdR { get; set; }
        public int StockItemIdR { get; set; }
        public Decimal NetR { get; set; }
        public Decimal PackR { get; set; }
        public Decimal? QtyR { get; set; }
        public Decimal? ReducedQtyR { get; set; }
        public byte[] RowVersionR { get; set; }
    }
}
