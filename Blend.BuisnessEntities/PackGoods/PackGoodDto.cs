using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.PackGoods
{
    public class PackGoodDto : BaseEntity
    {
        public int BlendId { get; set; }
        public string BlendNo { get; set; }
        public decimal BB { get; set; }
        public decimal GoodTea { get; set; }
        public decimal FlavouredTea { get; set; }
        public decimal PackGoodTea { get; set; }

    }
}
