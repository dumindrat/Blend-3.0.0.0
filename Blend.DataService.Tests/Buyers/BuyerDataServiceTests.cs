using System.Reflection;
using Blend.DataService.Buyers;
using Blend.DataService.Mapping;

namespace Blend.DataService.Tests.Buyers;

[TestClass]
public sealed class BuyerDataServiceTests
{
    [TestMethod]
    public void GetAllBuyers_AllSearchUsesEmptySearchParameterAndCompanyId()
    {
        var expected = new List<BuyerRecord> { new() { BuyerId = 4 } };
        var mapper = new RecordingMapper { Rows = expected };

        var actual = new BuyerDataService(mapper).GetAllBuyers("all", 2);

        Assert.AreSame(expected, actual);
        Assert.AreEqual("trn_BuyerGet", mapper.ProcedureName);
        Assert.AreEqual(string.Empty, Parameter<string>(mapper.Parameters, "serachstring"));
        Assert.AreEqual(2, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void CreateBuyer_MapsInsertParameters()
    {
        var mapper = new RecordingMapper();

        Assert.IsTrue(new BuyerDataService(mapper).CreateBuyer(new BuyerRecord
        {
            BuyerCode = "B1", BuyerName = "Name", Address = "Addr", Email = "b@example.com", User = "op", CompanyId = 3
        }));

        Assert.AreEqual("trn_BuyerInsert", mapper.ProcedureName);
        Assert.AreEqual("B1", Parameter<string>(mapper.Parameters, "BuyerCode"));
        Assert.AreEqual("Name", Parameter<string>(mapper.Parameters, "BuyerName"));
        Assert.AreEqual("Addr", Parameter<string>(mapper.Parameters, "Address"));
        Assert.AreEqual("b@example.com", Parameter<string>(mapper.Parameters, "Email"));
        Assert.AreEqual("op", Parameter<string>(mapper.Parameters, "User"));
        Assert.AreEqual(3, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void UpdateBuyer_MapsUpdateParametersIncludingRowVersion()
    {
        var mapper = new RecordingMapper();
        var version = new byte[] { 1, 2, 3 };

        Assert.IsTrue(new BuyerDataService(mapper).UpdateBuyer(new BuyerRecord
        {
            BuyerId = 8, BuyerCode = "B1", BuyerName = "Name", Address = "Addr", Email = "b@example.com", RowVersion = version
        }));

        Assert.AreEqual("trn_BuyerUpdate", mapper.ProcedureName);
        Assert.AreEqual(8, Parameter<int>(mapper.Parameters, "BId"));
        Assert.AreEqual("B1", Parameter<string>(mapper.Parameters, "BuyerCode"));
        Assert.AreEqual("Name", Parameter<string>(mapper.Parameters, "TeaBuyerName"));
        Assert.AreEqual("Addr", Parameter<string>(mapper.Parameters, "BAddress"));
        Assert.AreEqual("b@example.com", Parameter<string>(mapper.Parameters, "BEmail"));
        Assert.AreSame(version, Parameter<byte[]>(mapper.Parameters, "BRowVersion"));
    }

    [TestMethod]
    public void DeleteBuyer_UsesIdsParameter()
    {
        var mapper = new RecordingMapper();

        Assert.IsTrue(new BuyerDataService(mapper).DeleteBuyer(9));

        Assert.AreEqual("trn_BuyerDelete", mapper.ProcedureName);
        Assert.AreEqual(9, Parameter<int>(mapper.Parameters, "Ids"));
    }

    [TestMethod]
    public void BuyerRecord_NameColumnPopulatesBuyerName()
    {
        var record = new BuyerRecord { Name = "From column" };

        Assert.AreEqual("From column", record.BuyerName);
    }

    [TestMethod]
    public void Constructor_RejectsNullMapper() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new BuyerDataService(null!));

    [TestMethod]
    public void MapperFailuresPropagate()
    {
        var mapper = new RecordingMapper { Failure = new InvalidOperationException("db") };

        Assert.ThrowsExactly<InvalidOperationException>(() => new BuyerDataService(mapper).DeleteBuyer(1));
    }

    private static T Parameter<T>(object? parameters, string name) =>
        (T)parameters!.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!.GetValue(parameters)!;

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? ProcedureName { get; private set; }
        public object? Parameters { get; private set; }
        public IReadOnlyList<BuyerRecord> Rows { get; init; } = [];
        public Exception? Failure { get; init; }

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class =>
            throw new NotSupportedException();

        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null)
        {
            Record(procedureName, parameters);
            return (IReadOnlyList<T>)Rows;
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
