using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.BlendClose
{
    public interface IBlendCloseService
    {
        IList<BlendInfo> GetBlendList(string serchBlendNo);
        IList<BlendCloseDto> GetBlendCloseList(string serchBlendNo);
        BlendCloseDto GetBlendCloseInfoByBlendid(int blendId);
        IList<BlendCloseCancelRequest> GetBlendCancelRequest(string searchBlendNo);
        bool BlendClose(BlendCloseDto obj);
        bool BlendCloseCancelRequest(BlendCloseReversDto obj);
        bool ApproveBlendCloseCancel(BlendCloseReversDto obj);
    }
}
