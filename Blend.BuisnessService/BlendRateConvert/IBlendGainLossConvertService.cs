using Blend.BuisnessEntities.BlendClose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.BlendRateConvert
{
    public interface IBlendGainLossConvertService
    {
        decimal GetRate(int blendId);
        bool ConvertGain(BlendGainLossRateConvert obj);
        bool ConvertLoss(BlendGainLossRateConvert obj);
    }
}
