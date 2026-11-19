using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.GardenMarks;

public sealed class GardenMarkDataService : IGardenMarkDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public GardenMarkDataService(IStoredProcedureMapper mapper) =>
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public IReadOnlyList<GardenMarkRecord> GetGardenMarksList() =>
        _mapper.Query<GardenMarkRecord>("trn_GardenMarkGet");

    public bool InsertGardenMark(string gardenMark)
    {
        if (gardenMark is null) throw new ArgumentNullException(nameof(gardenMark));
        _mapper.Execute("trn_GardenMarkInsert", new { GardenMark = gardenMark });
        return true;
    }
}
