using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Documents;

public sealed class DocumentDataService : IDocumentDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public DocumentDataService(IStoredProcedureMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public IReadOnlyList<DocumentRecord> GetDocumentNumbers(int docType) =>
        _mapper.Query<DocumentRecord>("trn_DocumentGetNo", new { doctypes = docType });

    public IReadOnlyList<DocumentRecord> GetDocuments() =>
        _mapper.Query<DocumentRecord>("trn_DocumentGet");

    public bool ResetDocumentNumber(DocumentRecord document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));
        if (document.CompanyId is null) throw new ArgumentException("A company ID is required to reset a document number.", nameof(document));
        _mapper.Execute("trn_DocumentNoReset", new
        {
            doctype = document.DocType,
            DocNo = document.DocNo,
            Prefix = document.Prefix,
            DocFormat = document.DocFormat,
            User = document.User,
            CompanyId = document.CompanyId
        });
        return true;
    }
}
