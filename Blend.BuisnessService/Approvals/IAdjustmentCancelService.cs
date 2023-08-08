using Blend.BuisnessEntities.Approvals.Adjustments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Approvals
{
    public interface IAdjustmentCancelService
    {
        IEnumerable<AdjustmentCancelReq> GetStraightLines(string serchStr);
    }
}
