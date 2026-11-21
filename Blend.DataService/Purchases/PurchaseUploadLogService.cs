using System;
using Blend.DataService.Mapping;

namespace Blend.DataService.Purchases;

public sealed class PurchaseUploadLogService : IPurchaseUploadLogService
{
    private readonly IStoredProcedureMapper _mapper;

    public PurchaseUploadLogService(IStoredProcedureMapper mapper) => _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public void Insert(PurchaseUploadLog log)
    {
        if (log is null) throw new ArgumentNullException(nameof(log));
        _mapper.Execute("trn_SaleUploadLogInsert", log);
    }
}
