namespace Blend.BusinessService.Documents;

public interface IDocumentService
{
    DocumentNo GetDocumentNo(int docType, int companyId);
    IReadOnlyList<DocumentNo> GetDocumentData();
    IReadOnlyList<DocumentNo> GetDocumentDataByModule(int moduleId, int companyId);
    BusinessOperationResult ResetDocumentNo(DocumentNo documentInfo);
    bool IncrementDocumentNo(int docType);
}
