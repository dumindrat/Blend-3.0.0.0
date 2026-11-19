using Blend.DataService.GardenMarks;

namespace Blend.BusinessService.GardenMarks;

public sealed class GardenMarkService : IGardenMarkService
{
    private readonly IGardenMarkDataService _dataService;

    public GardenMarkService(IGardenMarkDataService dataService) =>
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public IReadOnlyList<GardenMark> GetGardenMarksList() =>
        _dataService.GetGardenMarksList().Select(record => new GardenMark { GardenMarkName = record.GardenMarkName }).ToArray();

    public BusinessOperationResult InsertGardenMark(string gardenMark)
    {
        ArgumentNullException.ThrowIfNull(gardenMark);
        return _dataService.InsertGardenMark(gardenMark)
            ? BusinessOperationResult.Success()
            : BusinessOperationResult.Failure("The garden mark could not be inserted.");
    }
}
