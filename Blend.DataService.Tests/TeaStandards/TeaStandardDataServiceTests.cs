using System.Reflection;
using Blend.DataService.Mapping;
using Blend.DataService.TeaStandards;

namespace Blend.DataService.Tests.TeaStandards;

[TestClass]
public sealed class TeaStandardDataServiceTests
{
    [TestMethod]
    public void GetAllStandards_UsesSearchStringAndCompanyIdParameters()
    {
        var rows = new List<TeaStandardRecord> { new() { StandardId = 3 } };
        var mapper = new RecordingMapper { Rows = rows };

        var actual = new TeaStandardDataService(mapper).GetAllStandards("all", 2);

        Assert.AreSame(rows, actual);
        Assert.AreEqual("trn_StandardGet", mapper.ProcedureName);
        Assert.AreEqual(string.Empty, Parameter<string>(mapper.Parameters, "searchstr"));
        Assert.AreEqual(2, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void GetAllStandards_PassesSearchText()
    {
        var mapper = new RecordingMapper();

        new TeaStandardDataService(mapper).GetAllStandards("Green", 3);

        Assert.AreEqual("Green", Parameter<string>(mapper.Parameters, "searchstr"));
        Assert.AreEqual(3, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void CreateStandard_MapsInsertParameters()
    {
        var mapper = new RecordingMapper();

        Assert.IsTrue(new TeaStandardDataService(mapper).CreateStandard(new TeaStandardRecord
        {
            StandardName = "Premium", User = "operator"
        }, 2));

        Assert.AreEqual("trn_StandardInsert", mapper.ProcedureName);
        Assert.AreEqual("Premium", Parameter<string>(mapper.Parameters, "StandardName"));
        Assert.AreEqual("operator", Parameter<string>(mapper.Parameters, "User"));
        Assert.AreEqual(2, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void UpdateStandard_MapsRowVersionAndUpdateParameters()
    {
        var mapper = new RecordingMapper();
        var rowVersion = new byte[] { 1, 2, 3 };

        Assert.IsTrue(new TeaStandardDataService(mapper).UpdateStandard(new TeaStandardRecord
        {
            StandardId = 17, StandardName = "Updated", User = "editor", RowVersion = rowVersion
        }));

        Assert.AreEqual("trn_StandardUpdate", mapper.ProcedureName);
        Assert.AreEqual(17, Parameter<int>(mapper.Parameters, "Id"));
        Assert.AreEqual("Updated", Parameter<string>(mapper.Parameters, "StandardName"));
        Assert.AreEqual("editor", Parameter<string>(mapper.Parameters, "User"));
        Assert.AreSame(rowVersion, Parameter<byte[]>(mapper.Parameters, "RowVersion"));
    }

    [TestMethod]
    public void DeleteStandard_UsesIdParameter()
    {
        var mapper = new RecordingMapper();

        Assert.IsTrue(new TeaStandardDataService(mapper).DeleteStandard(9));

        Assert.AreEqual("trn_StandardDelete", mapper.ProcedureName);
        Assert.AreEqual(9, Parameter<int>(mapper.Parameters, "Id"));
    }

    [TestMethod]
    public void ConstructorRejectsNullMapper() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new TeaStandardDataService(null!));

    [TestMethod]
    public void MapperFailuresPropagate()
    {
        var expected = new InvalidOperationException("Database failure.");
        var service = new TeaStandardDataService(new RecordingMapper { Failure = expected });

        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetAllStandards("all", 1)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.CreateStandard(new TeaStandardRecord(), 1)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.UpdateStandard(new TeaStandardRecord())));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteStandard(1)));
    }

    private static T Parameter<T>(object? parameters, string name) =>
        (T)parameters!.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!.GetValue(parameters)!;

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? ProcedureName { get; private set; }
        public object? Parameters { get; private set; }
        public IReadOnlyList<TeaStandardRecord> Rows { get; init; } = [];
        public Exception? Failure { get; init; }

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class =>
            throw new NotSupportedException();

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
