using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessEntities.LineSummary;

namespace Blend.BuisnessService.LineSummary
{
    public interface ILineSummaryService
    {
        IEnumerable<LineSummaryDto> GetLineDetails(string boxNo);
        IEnumerable<LineAllocationDto> GetLineAllocations(int id);
        IEnumerable<LineAdjustmentDto> GetLineAdjustments(int id);
        IEnumerable<LineTransHistory> GetLineTransHistory(int id);

    }
}
