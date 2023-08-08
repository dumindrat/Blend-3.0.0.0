using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data.Standard
{
    public class TeaStandard : BaseEntity
    {
        public long StandardId { get; set; }
        public string StandardName { get; set; }
    }

    public class TeaStandardForContract : TeaStandard
    {
        public string ExportQty { get; set; }
    }
}
