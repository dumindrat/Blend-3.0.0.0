using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Grades;

public sealed class GradeDataService : IGradeDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public GradeDataService(IStoredProcedureMapper mapper) =>
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public IReadOnlyList<GradeRecord> GetGradeList() =>
        _mapper.Query<GradeRecord>("trn_GradeGet");

    public bool InsertGrade(string grade)
    {
        if (grade is null) throw new ArgumentNullException(nameof(grade));
        _mapper.Execute("trn_GradeInsert", new { GradeName = grade });
        return true;
    }
}
