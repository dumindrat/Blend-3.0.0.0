using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data.Stock
{
    [Serializable]
    public class StockData : BaseEntity
    {
        public Int64 ReferenceNo { get; set; }
        public string ReserveNo { get; set; }
        public string BlendId { get; set; }
        public string Category { get; set; }
        public bool IsStraight { get; set; }
        public decimal Qty { get; set; }
        public string ReceivedVia { get; set; }
    }
}
