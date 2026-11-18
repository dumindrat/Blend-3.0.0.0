using System.Reflection;
using Blend.DataService.Contracts;
using Blend.DataService.Mapping;

namespace Blend.DataService.Tests.Contracts;

[TestClass]
public sealed class ContractDataServiceTests
{
    [TestMethod]
    public void ReadOperationsPassActualProcedureParameters()
    {
        var mapper = new RecordingMapper();
        var service = new ContractDataService(mapper);
        service.GetContracts("tea", 2);
        Check(mapper, "trn_ContractHeaderGet", ("searchstr", "tea"), ("CompanyId", 2));
        service.GetOpenContracts(2);
        Check(mapper, "trn_ContractOpenGet", ("CompanyId", 2));
        service.GetContractDetails(17);
        Check(mapper, "trn_ContractDetailGetByContractId", ("Id", 17L));
        service.GetAvailableStandards(17);
        Check(mapper, "trn_ContractAvailableStandards", ("Id", 17L));
        service.SearchAvailableStandards(17, "tea", 2);
        Check(mapper, "trn_ContractAvailableStandardsBySearch", ("Id", 17L), ("searchkey", "tea"), ("CompanyId", 2));
        service.GetBlends(17, 4);
        Check(mapper, "trn_BlendNoGetByContractAndStandard", ("ContractId", 17), ("StandardId", 4));
    }

    [TestMethod]
    public void WritesPassAllProcedureParametersIncludingVersions()
    {
        var mapper = new RecordingMapper();
        var service = new ContractDataService(mapper);
        var version = new byte[] { 1, 2, 3 };
        var contract = new ContractRecord { ContractId = 17, CompanyId = 2, ContractName = "C", BuyerId = 3, CompletionDate = "2026-11-17", User = "operator", RowVersion = version };
        var detail = new ContractDetailRecord { ContractId = 17, StandardId = 4, OrderQty = 12.5m, User = "operator", RowVersion = version };
        Assert.IsTrue(service.CreateContract(contract));
        Check(mapper, "trn_ContractHeaderInsert", ("ContractName", "C"), ("BuyerId", 3), ("CompletionDate", "2026-11-17"), ("CreateUser", "operator"), ("CompanyId", 2));
        Assert.IsTrue(service.UpdateContract(contract));
        Check(mapper, "trn_ContractHeaderUpdate", ("Id", 17L), ("ContractName", "C"), ("CompletionDate", "2026-11-17"), ("ModifiedUser", "operator"), ("RowVersion", version));
        Assert.IsTrue(service.DeleteContract(17));
        Check(mapper, "trn_ContractHeaderDelete", ("Id", 17L));
        Assert.IsTrue(service.CreateContractDetail(detail));
        Check(mapper, "trn_ContractDetailInsert", ("ContractId", 17L), ("StandardId", 4), ("ExportQty", 12.5m), ("CreateUser", "operator"));
        Assert.IsTrue(service.UpdateContractDetail(detail));
        Check(mapper, "trn_ContractDetailUpdate", ("CId", 17L), ("SId", 4), ("ExpQty", 12.5m), ("RowVersion", version), ("User", "operator"));
        Assert.IsTrue(service.DeleteContractDetail(detail));
        Check(mapper, "trn_ContractDetailDelete", ("ContractId", 17L), ("StandardId", 4), ("RowVersion", version));
    }

    [TestMethod]
    public void FailuresPropagateAndNullWritesAreRejected()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ContractDataService(null!));
        var mapper = new RecordingMapper();
        var service = new ContractDataService(mapper);
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateContract(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateContract(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateContractDetail(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateContractDetail(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.DeleteContractDetail(null!));
        var failure = new InvalidOperationException("Database failure");
        mapper.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetOpenContracts(2)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteContractDetail(new ContractDetailRecord())));
    }

    private static void Check(RecordingMapper mapper, string procedure, params (string Name, object Value)[] parameters)
    {
        Assert.AreEqual(procedure, mapper.Procedure);
        var properties = mapper.Parameters!.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual(parameters.Length, properties.Length);
        foreach (var (name, value) in parameters)
        {
            Assert.AreEqual(value, properties.Single(property => property.Name == name).GetValue(mapper.Parameters), name);
        }
    }

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? Procedure { get; private set; }
        public object? Parameters { get; private set; }
        public Exception? Failure { get; set; }
        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null) { Record(procedureName, parameters); return []; }
        public int Execute(string procedureName, object? parameters = null) { Record(procedureName, parameters); return -1; }
        private void Record(string procedure, object? parameters)
        {
            if (Failure is not null) throw Failure;
            Procedure = procedure;
            Parameters = parameters;
        }
    }
}
