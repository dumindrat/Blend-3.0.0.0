using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Adjustent
{
    public interface IAdjustmentService
    {
        IEnumerable<StraightLineStockItem> GetStraightLines(string serchStr, int companyId);
        IEnumerable<AdjutedLineDto> GetAdjustedLineById(int Id);
        IEnumerable<AdjutedLineDto> GetAdjustedLineByBoxNo(string boxNo);
        IEnumerable<AdjutLineCancelRequest> GetAdjusteLineCancelRequests(string serchStr);
        bool AddToReject(AdjutLineDto obj);
        bool AddToShort(AdjutLineDto obj);
        bool CancelRequest(AdjutLineCancelRequest obj);
        bool ApproveCancelRequest(AdjutLineCancelRequest obj);

        IEnumerable<PrintNoteDto> GetAdjustedLinesByBrokerAndType(int brokerId, int type, int status);
        bool SettlePayment(int adjustmentId, string user);

    }
}
