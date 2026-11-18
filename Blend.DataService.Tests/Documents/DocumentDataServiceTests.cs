using System.Reflection;
using Blend.DataService.Documents;
using Blend.DataService.Mapping;

namespace Blend.DataService.Tests.Documents;

[TestClass]
public sealed class DocumentDataServiceTests
{
    [TestMethod]
    public void GetDocumentNumbers_UsesDocumentTypeParameter()
    {
        var records = new List<DocumentRecord> { new() { DocType = 3, CompanyId = 2 } };
        var mapper = new RecordingMapper { Rows = records };

        Assert.AreSame(records, new DocumentDataService(mapper).GetDocumentNumbers(3));
        Assert.AreEqual("trn_DocumentGetNo", mapper.ProcedureName);
        Assert.AreEqual(3, Parameter<int>(mapper.Parameters, "doctypes"));
    }

    [TestMethod]
    public void GetDocuments_UsesParameterlessProcedure()
    {
        var mapper = new RecordingMapper();

        new DocumentDataService(mapper).GetDocuments();

        Assert.AreEqual("trn_DocumentGet", mapper.ProcedureName);
        Assert.IsNull(mapper.Parameters);
    }

    [TestMethod]
    public void ResetDocumentNumber_MapsAllParameters()
    {
        var mapper = new RecordingMapper();
        var document = new DocumentRecord { DocType = 3, DocNo = 27, Prefix = "P", DocFormat = "0000", User = "operator", CompanyId = 2 };

        Assert.IsTrue(new DocumentDataService(mapper).ResetDocumentNumber(document));

        Assert.AreEqual("trn_DocumentNoReset", mapper.ProcedureName);
        Assert.AreEqual(3, Parameter<int>(mapper.Parameters, "doctype"));
        Assert.AreEqual(27, Parameter<int>(mapper.Parameters, "DocNo"));
        Assert.AreEqual("P", Parameter<string>(mapper.Parameters, "Prefix"));
        Assert.AreEqual("0000", Parameter<string>(mapper.Parameters, "DocFormat"));
        Assert.AreEqual("operator", Parameter<string>(mapper.Parameters, "User"));
        Assert.AreEqual(2, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void NullArgumentsAndMapperFailuresPropagate()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new DocumentDataService(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DocumentDataService(new RecordingMapper()).ResetDocumentNumber(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new DocumentDataService(new RecordingMapper()).ResetDocumentNumber(new DocumentRecord()));
        var expected = new InvalidOperationException("Database failure.");
        var service = new DocumentDataService(new RecordingMapper { Failure = expected });
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocumentNumbers(1)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetDocuments()));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.ResetDocumentNumber(new DocumentRecord { CompanyId = 1 })));
    }

    private static T Parameter<T>(object? parameters, string name) =>
        (T)parameters!.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)!.GetValue(parameters)!;

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? ProcedureName { get; private set; }
        public object? Parameters { get; private set; }
        public IReadOnlyList<DocumentRecord> Rows { get; init; } = [];
        public Exception? Failure { get; init; }

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();

        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null)
        {
            Record(procedureName, parameters);
            return (IReadOnlyList<T>)(object)Rows;
        }

        public int Execute(string procedureName, object? parameters = null)
        {
            Record(procedureName, parameters);
            return 1;
        }

        private void Record(string procedureName, object? parameters)
        {
            if (Failure is not null) throw Failure;
            ProcedureName = procedureName;
            Parameters = parameters;
        }
    }
}
