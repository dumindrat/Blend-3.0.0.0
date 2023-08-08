using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Report;

namespace Blend.BuisnessService.Reports.Blend
{
    public class BlendReportService : BaseManagement<BlendSheetDto>, IBlendReportService
    {
        public IList<BuisnessEntities.BlendCreation.BlendSheetDto> GetBlendSheet(int blendId)
        {
            var mapper = new DataMapper<BlendSheetDto>("GetBlendSheet", new Hashtable() { { "BlendId", blendId } });

            var result = mapper.ObjectList();
            return result;
        }


        public BlendSheetParam GetBlendSheetParam(int blendId)
        {
            var mapper = new DataMapper<BlendSheetParam>("GetBlendSheetParam", new Hashtable() { { "BlendId", blendId } });

            var result = mapper.Object();
            return result;
        }


        public IList<BuisnessEntities.Report.BlendCanister> GetBlendCanister(int blendId)
        {
            var mapper = new DataMapper<BlendCanister>("GetBlendCanister", new Hashtable() { { "BlendId", blendId } });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
