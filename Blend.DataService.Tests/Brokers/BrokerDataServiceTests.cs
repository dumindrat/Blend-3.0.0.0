using System.Collections.Generic;
using System.Reflection;
using Blend.DataService.Brokers;
using Blend.DataService.Mapping;

namespace Blend.DataService.Tests.Brokers;

[TestClass]
public sealed class BrokerDataServiceTests
{
    [TestMethod]
    public void GetBrokerById_UsesBrokerIdProcedureAndReturnsMappedRecord()
    {
        var expected = new BrokerRecord { BrokerId = 42, BrokerName = "Example" };
        var mapper = new RecordingStoredProcedureMapper { SingleResult = expected };
        var service = new BrokerDataService(mapper);

        var actual = service.GetBrokerById(42);

        Assert.AreSame(expected, actual);
        Assert.AreEqual("trn_BrokerGetById", mapper.ProcedureName);
        Assert.AreEqual(42, Parameter<int>(mapper.Parameters, "BrokerId"));
    }

    [TestMethod]
    public void GetAllBrokers_AllSearchUsesEmptyNameAndCompanyId()
    {
        var expected = new List<BrokerRecord> { new() { BrokerId = 7 } };
        var mapper = new RecordingStoredProcedureMapper { Rows = expected };
        var service = new BrokerDataService(mapper);

        var actual = service.GetAllBrokers("all", 12);

        Assert.AreSame(expected, actual);
        Assert.AreEqual("trn_BrokerGet", mapper.ProcedureName);
        Assert.AreEqual(string.Empty, Parameter<string>(mapper.Parameters, "BrokerName"));
        Assert.AreEqual(12, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void GetAllBrokers_PassesSearchTextAndCompanyId()
    {
        var mapper = new RecordingStoredProcedureMapper();
        var service = new BrokerDataService(mapper);

        service.GetAllBrokers("North", 3);

        Assert.AreEqual("North", Parameter<string>(mapper.Parameters, "BrokerName"));
        Assert.AreEqual(3, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void CreateBroker_UsesInsertProcedureAndMapsAllParameters()
    {
        var mapper = new RecordingStoredProcedureMapper();
        var service = new BrokerDataService(mapper);
        var broker = new BrokerRecord
        {
            BrokerCode = "ABC",
            BrokerName = "Broker name",
            Address = "Address",
            Email = "broker@example.com",
            Tel = "123456",
            ContactPerson = "Contact",
            User = "operator",
            CompanyId = 9
        };

        Assert.IsTrue(service.CreateBroker(broker));

        Assert.AreEqual("trn_BrokerInsert", mapper.ProcedureName);
        Assert.AreEqual("ABC", Parameter<string>(mapper.Parameters, "BCode"));
        Assert.AreEqual("Broker name", Parameter<string>(mapper.Parameters, "BName"));
        Assert.AreEqual("Address", Parameter<string>(mapper.Parameters, "BAddress"));
        Assert.AreEqual("broker@example.com", Parameter<string>(mapper.Parameters, "BEmail"));
        Assert.AreEqual("123456", Parameter<string>(mapper.Parameters, "BTel"));
        Assert.AreEqual("Contact", Parameter<string>(mapper.Parameters, "BContactPerson"));
        Assert.AreEqual("operator", Parameter<string>(mapper.Parameters, "User"));
        Assert.AreEqual(9, Parameter<int>(mapper.Parameters, "CompanyId"));
    }

    [TestMethod]
    public void UpdateBroker_UsesUpdateProcedureAndIncludesRowVersion()
    {
        var mapper = new RecordingStoredProcedureMapper();
        var service = new BrokerDataService(mapper);
        var rowVersion = new byte[] { 1, 2, 3 };
        var broker = new BrokerRecord
        {
            BrokerId = 24,
            BrokerCode = "XYZ",
            BrokerName = "Updated",
            Address = "New address",
            Email = "updated@example.com",
            Tel = "654321",
            ContactPerson = "New contact",
            RowVersion = rowVersion,
            User = "editor"
        };

        Assert.IsTrue(service.UpdateBroker(broker));

        Assert.AreEqual("trn_BrokerUpdate", mapper.ProcedureName);
        Assert.AreEqual(24, Parameter<int>(mapper.Parameters, "BId"));
        Assert.AreEqual("XYZ", Parameter<string>(mapper.Parameters, "BCode"));
        Assert.AreEqual("Updated", Parameter<string>(mapper.Parameters, "BName"));
        Assert.AreEqual("New address", Parameter<string>(mapper.Parameters, "BAddress"));
        Assert.AreEqual("updated@example.com", Parameter<string>(mapper.Parameters, "BEmail"));
        Assert.AreEqual("654321", Parameter<string>(mapper.Parameters, "BTel"));
        Assert.AreEqual("New contact", Parameter<string>(mapper.Parameters, "BContactPerson"));
        Assert.AreSame(rowVersion, Parameter<byte[]>(mapper.Parameters, "RowVersion"));
        Assert.AreEqual("editor", Parameter<string>(mapper.Parameters, "User"));
    }

    [TestMethod]
    public void DeleteBroker_UsesDeleteProcedureAndBrokerId()
    {
        var mapper = new RecordingStoredProcedureMapper();
        var service = new BrokerDataService(mapper);

        Assert.IsTrue(service.DeleteBroker(8));

        Assert.AreEqual("trn_BrokerDelete", mapper.ProcedureName);
        Assert.AreEqual(8, Parameter<int>(mapper.Parameters, "BId"));
    }

    [TestMethod]
    public void Constructor_RejectsNullMapper()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new BrokerDataService(null!));
    }

    private static T Parameter<T>(object? parameters, string name)
    {
        Assert.IsNotNull(parameters);
        var property = parameters.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"Parameter '{name}' was not supplied.");
        return (T)property.GetValue(parameters)!;
    }

    private sealed class RecordingStoredProcedureMapper : IStoredProcedureMapper
    {
        public string? ProcedureName { get; private set; }
        public object? Parameters { get; private set; }
        public BrokerRecord? SingleResult { get; init; }
        public IReadOnlyList<BrokerRecord> Rows { get; init; } = Array.Empty<BrokerRecord>();

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class
        {
            Record(procedureName, parameters);
            return SingleResult as T;
        }

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
            ProcedureName = procedureName;
            Parameters = parameters;
        }
    }
}
