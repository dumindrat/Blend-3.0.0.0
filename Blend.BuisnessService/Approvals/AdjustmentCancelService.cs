using Blend.BuisnessEntities.Approvals.Adjustments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Approvals
{
    public class AdjustmentCancelService : BaseManagement<AdjustmentCancelReq>, IAdjustmentCancelService
    {
        public IEnumerable<BuisnessEntities.Approvals.Adjustments.AdjustmentCancelReq> GetStraightLines(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var result = Mapper.QueryForList<AdjustmentCancelReq>("GetLinesForCancelApprove", new Hashtable() { { "BoxNo", searchBy } });
            return result;
        }
    }
}
