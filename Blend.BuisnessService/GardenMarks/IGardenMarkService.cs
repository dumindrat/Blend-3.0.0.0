using Blend.BuisnessEntities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.GardenMarks
{
    public interface IGardenMarkService
    {
        IEnumerable<GardenMark> GetGardenMarksList();
        bool InsertGardenMark(string gardenMark);
    }
}
