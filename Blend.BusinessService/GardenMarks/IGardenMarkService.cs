namespace Blend.BusinessService.GardenMarks;

public interface IGardenMarkService
{
    IReadOnlyList<GardenMark> GetGardenMarksList();
    BusinessOperationResult InsertGardenMark(string gardenMark);
}
