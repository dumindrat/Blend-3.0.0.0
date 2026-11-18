using System.Collections.Generic;

namespace Blend.DataService.Documents;

public interface IDocumentDataService
{
    IReadOnlyList<DocumentRecord> GetDocumentNumbers(int docType);
    IReadOnlyList<DocumentRecord> GetDocuments();
    bool ResetDocumentNumber(DocumentRecord document);
}
