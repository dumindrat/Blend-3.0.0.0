using Blend.BuisnessEntities.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TeaStandards
{
    public interface ITeaStandardService
    {
        TeaStandard GetTeaStandardById(int id);
        IList<TeaStandard> GetAllTeaStandard(string searchstr);
        bool CreateStandard(TeaStandard obj);
        bool UpdateStandard(TeaStandard obj);
        bool Deletetandard(TeaStandard obj);

    }
}
