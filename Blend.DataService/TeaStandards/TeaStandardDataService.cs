using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.TeaStandards;

public sealed class TeaStandardDataService : ITeaStandardDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public TeaStandardDataService(IStoredProcedureMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public IReadOnlyList<TeaStandardRecord> GetAllStandards(string searchText, int companyId)
    {
        var searchBy = searchText == "all" ? string.Empty : searchText;
        return _mapper.Query<TeaStandardRecord>("trn_StandardGet", new { searchstr = searchBy, CompanyId = companyId });
    }

    public bool CreateStandard(TeaStandardRecord standard, int companyId)
    {
        if (standard is null) throw new ArgumentNullException(nameof(standard));
        _mapper.Execute("trn_StandardInsert", new
        {
            StandardName = standard.StandardName,
            User = standard.User,
            CompanyId = companyId
        });
        return true;
    }

    public bool UpdateStandard(TeaStandardRecord standard)
    {
        if (standard is null) throw new ArgumentNullException(nameof(standard));
        _mapper.Execute("trn_StandardUpdate", new
        {
            Id = standard.StandardId,
            StandardName = standard.StandardName,
            User = standard.User,
            RowVersion = standard.RowVersion
        });
        return true;
    }

    public bool DeleteStandard(int standardId)
    {
        _mapper.Execute("trn_StandardDelete", new { Id = standardId });
        return true;
    }
}
