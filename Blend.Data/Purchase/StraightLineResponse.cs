using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blend.Data.Response;

namespace Blend.Data.Purchase
{
    public class StraightLineResponse
    {
        public IList<StraightLine> StraightLines { get; set; }
        public ResponseData Message { get; set; }
    }
}
