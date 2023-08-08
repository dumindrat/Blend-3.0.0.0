using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Download.Asycuda
{
    public class AsycudaDataSL
    {
        public string StockId { get; set; }
        public string SaleDate { get; set; }
        public string SaleCode { get; set; }
        public string BrokerCode { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string GardenMark { get; set; }
        public string Grade { get; set; }
        public string Rate { get; set; }
        public string Qty { get; set; }
        public string Value { get; set; }

    }

    public class AsycudaDataBL
    {
        public string ClosedDate { get; set; }
        public string BlendNo { get; set; }
        public string BB { get; set; }
        public string BG { get; set; }
        public string ClosedRate { get; set; }
    }
}
