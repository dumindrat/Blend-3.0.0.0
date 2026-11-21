using Blend.DataService.Documents;

namespace Blend.BusinessService.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentDataService _dataService;

    public DocumentService(IDocumentDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public DocumentNo GetDocumentNo(int docType, int companyId)
    {
        var record = _dataService.GetDocumentNumbers(docType).Single(item => item.CompanyId == companyId);
        var document = ToDocument(record);
        document.FormattedDocNo = FormatDocumentNumber(record);
        return document;
    }

    internal static string FormatDocumentNumber(DocumentRecord record)
    {
        if (record.DocFormat is null)
        {
            throw new InvalidOperationException("The document number has no format.");
        }

        var paddedNumber = record.DocFormat + record.DocNo;
        return paddedNumber.Substring(paddedNumber.Length - record.DocFormat.Length) + record.Prefix;
    }

    public IReadOnlyList<DocumentNo> GetDocumentData() =>
        _dataService.GetDocuments().Select(ToDocument).ToArray();

    public IReadOnlyList<DocumentNo> GetDocumentDataByModule(int moduleId, int companyId) =>
        _dataService.GetDocuments()
            .Where(item => item.ModuleId == moduleId && item.CompanyId == companyId)
            .Select(ToDocument)
            .ToArray();

    public BusinessOperationResult ResetDocumentNo(DocumentNo documentInfo)
    {
        ArgumentNullException.ThrowIfNull(documentInfo);
        ArgumentException.ThrowIfNullOrEmpty(documentInfo.DocFormat);
        ArgumentException.ThrowIfNullOrEmpty(documentInfo.Prefix);
        if (documentInfo.CompanyId is null)
        {
            throw new ArgumentException("A company ID is required to reset a document number.", nameof(documentInfo));
        }

        return _dataService.ResetDocumentNumber(ToRecord(documentInfo))
            ? BusinessOperationResult.Success()
            : BusinessOperationResult.Failure("The document number could not be reset.");
    }

    public bool IncrementDocumentNo(int docType) => throw new NotImplementedException();

    private static DocumentNo ToDocument(DocumentRecord record) => new()
    {
        Id = record.Id,
        DocType = record.DocType,
        DocNo = record.DocNo,
        Prefix = record.Prefix,
        DocFormat = record.DocFormat,
        Description = record.Description,
        ModuleId = record.ModuleId,
        CompanyId = record.CompanyId,
        User = record.User
    };

    private static DocumentRecord ToRecord(DocumentNo document) => new()
    {
        Id = document.Id,
        DocType = document.DocType,
        DocNo = document.DocNo,
        Prefix = document.Prefix,
        DocFormat = document.DocFormat,
        Description = document.Description,
        ModuleId = document.ModuleId,
        CompanyId = document.CompanyId,
        User = document.User
    };
}
