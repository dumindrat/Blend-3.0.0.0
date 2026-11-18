using Blend.BusinessService.Documents;
using Blend.DataService.Documents;

namespace Blend.BusinessService.Tests.Documents;

[TestClass]
public sealed class DocumentServiceTests
{
    [TestMethod]
    public void GetDocumentNo_FormatsNumberAndSelectsCompany()
    {
        var data = new FakeDocumentDataService
        {
            Numbers = [
                new() { DocType = 5, CompanyId = 1, DocNo = 5, DocFormat = "0000", Prefix = "A" },
                new() { DocType = 5, CompanyId = 2, DocNo = 42, DocFormat = "0000", Prefix = "B" }
            ]
        };

        var result = new DocumentService(data).GetDocumentNo(5, 2);

        Assert.AreEqual(5, data.RequestedDocType);
        Assert.AreEqual(2, result.CompanyId);
        Assert.AreEqual("0042B", result.FormattedDocNo);
    }

    [TestMethod]
    public void GetDocumentNo_PreservesLegacyTruncationAndEmptyFormat()
    {
        var data = new FakeDocumentDataService
        {
            Numbers = [new() { CompanyId = 1, DocNo = 12345, DocFormat = "000", Prefix = "P" }]
        };
        var service = new DocumentService(data);
        Assert.AreEqual("345P", service.GetDocumentNo(1, 1).FormattedDocNo);
        data.Numbers = [new() { CompanyId = 1, DocNo = 42, DocFormat = "", Prefix = "P" }];
        Assert.AreEqual("P", service.GetDocumentNo(1, 1).FormattedDocNo);
    }

    [TestMethod]
    public void GetDocumentNo_RejectsMissingOrDuplicateCompanyAndMissingFormat()
    {
        var data = new FakeDocumentDataService();
        var service = new DocumentService(data);
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentNo(1, 1));
        data.Numbers = [new() { CompanyId = 1 }, new() { CompanyId = 1 }];
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentNo(1, 1));
        data.Numbers = [new() { CompanyId = 1, DocFormat = null }];
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentNo(1, 1));
    }

    [TestMethod]
    public void GetDocuments_MapsRecordsAndFiltersByModuleAndCompany()
    {
        var data = new FakeDocumentDataService
        {
            Documents = [
                new() { Id = 7, DocType = 3, DocNo = 12, DocFormat = "000", Prefix = "X", Description = "Order", ModuleId = 1, CompanyId = 2, User = "operator" },
                new() { Id = 8, ModuleId = 1, CompanyId = 1 },
                new() { Id = 9, ModuleId = 2, CompanyId = 2 },
                new() { Id = 10, ModuleId = null, CompanyId = null }
            ]
        };
        var service = new DocumentService(data);

        var all = service.GetDocumentData();
        var filtered = service.GetDocumentDataByModule(1, 2);

        Assert.AreEqual(4, all.Count);
        Assert.AreEqual(1, filtered.Count);
        var document = filtered[0];
        Assert.AreEqual(7, document.Id);
        Assert.AreEqual(3, document.DocType);
        Assert.AreEqual(12, document.DocNo);
        Assert.AreEqual("000", document.DocFormat);
        Assert.AreEqual("X", document.Prefix);
        Assert.AreEqual("Order", document.Description);
        Assert.AreEqual(1, document.ModuleId);
        Assert.AreEqual(2, document.CompanyId);
        Assert.AreEqual("operator", document.User);
    }

    [TestMethod]
    public void ResetDocumentNo_MapsInputsAndReportsFailure()
    {
        var data = new FakeDocumentDataService();
        var service = new DocumentService(data);
        var document = new DocumentNo { DocType = 3, DocNo = 0, DocFormat = "0000", Prefix = "P", CompanyId = 2, User = "operator" };

        Assert.IsTrue(service.ResetDocumentNo(document).Succeeded);
        Assert.IsNotNull(data.Reset);
        Assert.AreEqual(3, data.Reset.DocType);
        Assert.AreEqual(0, data.Reset.DocNo);
        Assert.AreEqual("0000", data.Reset.DocFormat);
        Assert.AreEqual("P", data.Reset.Prefix);
        Assert.AreEqual(2, data.Reset.CompanyId);
        Assert.AreEqual("operator", data.Reset.User);
        data.ResetResult = false;
        Assert.AreEqual("The document number could not be reset.", service.ResetDocumentNo(document).ErrorMessage);
    }

    [TestMethod]
    public void ResetDocumentNo_RejectsMissingFieldsAndCompany()
    {
        var service = new DocumentService(new FakeDocumentDataService());
        Assert.ThrowsExactly<ArgumentNullException>(() => service.ResetDocumentNo(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.ResetDocumentNo(new DocumentNo { Prefix = "P", CompanyId = 1 }));
        Assert.ThrowsExactly<ArgumentException>(() => service.ResetDocumentNo(new DocumentNo { DocFormat = "", Prefix = "P", CompanyId = 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.ResetDocumentNo(new DocumentNo { DocFormat = "000", CompanyId = 1 }));
        Assert.ThrowsExactly<ArgumentException>(() => service.ResetDocumentNo(new DocumentNo { DocFormat = "000", Prefix = "P" }));
    }

    [TestMethod]
    public void ConstructorAndFailuresAreExplicit()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new DocumentService(null!));
        var expected = new InvalidOperationException("Database failure.");
        var service = new DocumentService(new FakeDocumentDataService { Failure = expected });
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentNo(1, 1)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentData()));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentDataByModule(1, 1)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.ResetDocumentNo(new DocumentNo { DocFormat = "0", Prefix = "P", CompanyId = 1 })));
        Assert.ThrowsExactly<NotImplementedException>(() => service.IncrementDocumentNo(1));
    }

    private sealed class FakeDocumentDataService : IDocumentDataService
    {
        public IReadOnlyList<DocumentRecord> Numbers { get; set; } = [];
        public IReadOnlyList<DocumentRecord> Documents { get; init; } = [];
        public int? RequestedDocType { get; private set; }
        public DocumentRecord? Reset { get; private set; }
        public bool ResetResult { get; set; } = true;
        public Exception? Failure { get; init; }

        public IReadOnlyList<DocumentRecord> GetDocumentNumbers(int docType)
        {
            if (Failure is not null) throw Failure;
            RequestedDocType = docType;
            return Numbers;
        }

        public IReadOnlyList<DocumentRecord> GetDocuments()
        {
            if (Failure is not null) throw Failure;
            return Documents;
        }

        public bool ResetDocumentNumber(DocumentRecord document)
        {
            if (Failure is not null) throw Failure;
            Reset = document;
            return ResetResult;
        }
    }
}
