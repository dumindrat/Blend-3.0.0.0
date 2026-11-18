using Blend.DataService.TeaStandards;

namespace Blend.BusinessService.TeaStandards;

public sealed class TeaStandardService : ITeaStandardService
{
    private readonly ITeaStandardDataService _dataService;

    public TeaStandardService(ITeaStandardDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public IReadOnlyList<TeaStandard> GetAllStandards(string searchText, int companyId) =>
        _dataService.GetAllStandards(searchText, companyId).Select(ToStandard).ToArray();

    public BusinessOperationResult CreateStandard(TeaStandard standard, int companyId)
    {
        ArgumentNullException.ThrowIfNull(standard);
        return ToOperationResult(_dataService.CreateStandard(ToRecord(standard), companyId), "The standard could not be created.");
    }

    public BusinessOperationResult UpdateStandard(TeaStandard standard)
    {
        ArgumentNullException.ThrowIfNull(standard);
        return ToOperationResult(_dataService.UpdateStandard(ToRecord(standard)), "The standard could not be updated.");
    }

    public BusinessOperationResult DeleteStandard(int standardId) =>
        ToOperationResult(_dataService.DeleteStandard(standardId), "The standard could not be deleted.");

    private static BusinessOperationResult ToOperationResult(bool succeeded, string failureMessage) =>
        succeeded ? BusinessOperationResult.Success() : BusinessOperationResult.Failure(failureMessage);

    private static TeaStandard ToStandard(TeaStandardRecord record) => new()
    {
        StandardId = record.StandardId,
        StandardName = record.StandardName,
        RowVersion = record.RowVersion,
        User = record.User
    };

    private static TeaStandardRecord ToRecord(TeaStandard standard) => new()
    {
        StandardId = standard.StandardId,
        StandardName = standard.StandardName,
        RowVersion = standard.RowVersion,
        User = standard.User
    };
}
