using System.Collections.Generic;

namespace Blend.DataService.Grades;

public interface IGradeDataService
{
    IReadOnlyList<GradeRecord> GetGradeList();
    bool InsertGrade(string grade);
}
