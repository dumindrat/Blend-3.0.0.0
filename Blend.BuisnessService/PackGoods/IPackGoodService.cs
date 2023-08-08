
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.PackGoods;

namespace Blend.BuisnessService.PackGoods
{
    public interface IPackGoodService
    {
        bool ConvertPackGoods(PackGoodDto obj);
    }
}
