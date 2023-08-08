using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data.Purchase
{
    public class StraightLine : BaseEntity
    {
        public string ReservedNo { get; set; }
        public int BrokerId { get; set; }
        public string LotNo { get; set; }
        public string InvoiceNo { get; set; }
        public string SaleType { get; set; }
        public string SaleCode { get; set; }
        public string SaleDate { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Packet { get; set; }
        public decimal Rate { get; set; }
        public decimal Weight { get; set; }
        public decimal ReduceQty { get; set; }
        public bool IsVoid { get; set; }
        public bool StockReceived { get; set; }
        public string StockReceivedDate { get; set; }
        public string StoreLocation { get; set; }
        public int CollectionNoteNo { get; set; }
    }

    public class ResponseStraightLine
    {
        public string Message { get; set; }

        public IList<StraightLine> EBook { get; set; }
    }
}
