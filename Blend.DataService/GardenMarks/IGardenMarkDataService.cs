using System.Collections.Generic;

namespace Blend.DataService.GardenMarks;

public interface IGardenMarkDataService
{
    IReadOnlyList<GardenMarkRecord> GetGardenMarksList();
    bool InsertGardenMark(string gardenMark);
}
