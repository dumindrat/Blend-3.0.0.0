using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.RejectLine
{
    public class RejectLineService : BaseManagement<AdjutLineDto>, IRejectLineService
    {
        public IEnumerable<StraightLineStockItem> GetStraightLines(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var mapper = new DataMapper<StraightLineStockItem>("GetLinesForReject", new Hashtable() { { "BoxNo", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool RejectLine(AdjutLineDto obj)
        {
            throw new NotImplementedException();
        }
    }
}
