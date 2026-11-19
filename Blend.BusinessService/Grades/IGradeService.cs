namespace Blend.BusinessService.Grades;

public interface IGradeService
{
    IReadOnlyList<Grade> GetGradeList();
    BusinessOperationResult InsertGrade(string grade);
}
