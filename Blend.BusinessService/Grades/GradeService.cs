using Blend.DataService.Grades;

namespace Blend.BusinessService.Grades;

public sealed class GradeService : IGradeService
{
    private readonly IGradeDataService _dataService;

    public GradeService(IGradeDataService dataService) =>
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public IReadOnlyList<Grade> GetGradeList() =>
        _dataService.GetGradeList().Select(record => new Grade { GradeName = record.GradeName }).ToArray();

    public BusinessOperationResult InsertGrade(string grade)
    {
        ArgumentNullException.ThrowIfNull(grade);
        return _dataService.InsertGrade(grade)
            ? BusinessOperationResult.Success()
            : BusinessOperationResult.Failure("The grade could not be inserted.");
    }
}
