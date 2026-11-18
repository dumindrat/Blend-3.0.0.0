using System.Collections.Generic;

namespace Blend.DataService.TeaStandards;

public interface ITeaStandardDataService
{
    IReadOnlyList<TeaStandardRecord> GetAllStandards(string searchText, int companyId);
    bool CreateStandard(TeaStandardRecord standard, int companyId);
    bool UpdateStandard(TeaStandardRecord standard);
    bool DeleteStandard(int standardId);
}
