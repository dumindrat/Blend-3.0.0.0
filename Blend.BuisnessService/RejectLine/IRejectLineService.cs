using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.RejectLine
{
    public interface IRejectLineService
    {
        IEnumerable<StraightLineStockItem> GetStraightLines(string serchStr);
        bool RejectLine(AdjutLineDto obj);
    }
}
