using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    public class BlendItemSummaryDto
    {
        public List<BlendItemSummary> BlendItemSummaryList { get; set; }
    }

    public class BlendItemSummary
    {
        public string LineType { get; set; }
        public int LineCount { get; set; }
        public Decimal Qty { get; set; }
        public decimal Value { get; set; }
        public List<BlendItemDisplay> BlendItems { get; set; }
    }

    public class BlendItemDisplay
    {
        public string BoxNo { get; set; }
        public string SaleDate { get; set; }
    }

    public class BlendItemAllocation
    {
        public string BlendNo { get; set; }
        public decimal Qty { get; set; }
        public decimal Packs { get; set; }
        public string CreateDate { get; set; }
    }

    
}
