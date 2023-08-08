using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Report;

namespace Blend.BuisnessService.Reports.Blend
{
    public interface IBlendReportService
    {
        IList<BlendSheetDto> GetBlendSheet(int blendId);
        BlendSheetParam GetBlendSheetParam(int blendId);
        IList<BlendCanister> GetBlendCanister(int blendId);
    }
}
