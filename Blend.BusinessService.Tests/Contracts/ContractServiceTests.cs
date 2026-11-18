using Blend.BusinessService.Contracts;
using Blend.DataService.Contracts;

namespace Blend.BusinessService.Tests.Contracts;

[TestClass]
public sealed class ContractServiceTests
{
    [TestMethod]
    public void SearchesNormalizeLegacyTokensAndPassCompany()
    {
        var data = new FakeData();
        var service = new ContractService(data);
        service.GetContractList("all", 2);
        Assert.AreEqual(("", 2), data.Search);
        service.GetContractList("A~B", 3);
        Assert.AreEqual(("A/B", 3), data.Search);
        service.GetOpenContractList(2);
        Assert.AreEqual(2, data.CompanyId);
        service.GetAvailableStandardsByIdAndSearchStr(17, "all", 3);
        Assert.AreEqual((17L, "", 3), data.StandardSearch);
        service.GetAvailableStandardsByIdAndSearchStr(17, "A~B", 2);
        Assert.AreEqual((17L, "A~B", 2), data.StandardSearch);
        Assert.ThrowsExactly<ArgumentNullException>(() => service.GetContractList(null!, 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.GetAvailableStandardsByIdAndSearchStr(1, null!, 1));
    }

    [TestMethod]
    public void ReadResultsMapAllRelevantFields()
    {
        var version = new byte[] { 1, 2 };
        var data = new FakeData
        {
            Contracts = [new ContractRecord { ContractId = 17, CompanyId = 2, ContractName = "C", Contract = "ref", BuyerId = 3, BuyerCode = "B", BuyerName = "Buyer", CompletionDate = "2026-11-17", ModifiedDate = "today", IsClosed = true, CloseStatus = "Closed", RowVersion = version, User = "operator" }],
            Details = [new ContractDetailRecord { ContractId = 17, StandardId = 4, StandardName = "Tea", OrderQty = 12.5m, RowVersion = version, User = "operator" }],
            Standards = [new AvailableStandardRecord { ContractId = 17, StandardId = 4, StandardName = "Tea", OrderQty = "0" }],
            Blends = [new ContractBlendRecord { BlendNo = "B1", ClosedStatus = "Open" }]
        };
        var service = new ContractService(data);
        var contract = service.GetContractList("all", 2).Single();
        Assert.AreEqual(17L, contract.ContractId); Assert.AreEqual(2, contract.CompanyId); Assert.AreEqual("C", contract.ContractName);
        Assert.AreEqual("ref", contract.Contract); Assert.AreEqual(3, contract.BuyerId); Assert.AreEqual("B", contract.BuyerCode);
        Assert.AreEqual("Buyer", contract.BuyerName); Assert.AreEqual("2026-11-17", contract.CompletionDate); Assert.AreEqual("today", contract.ModifiedDate);
        Assert.IsTrue(contract.IsClosed); Assert.AreEqual("Closed", contract.CloseStatus); Assert.AreSame(version, contract.RowVersion); Assert.AreEqual("operator", contract.User);
        var open = service.GetOpenContractList(3).Single();
        Assert.AreEqual(17L, open.ContractId);
        Assert.AreEqual(3, open.CompanyId);
        var detail = service.GetBuyerContractDetailById(17).Single();
        Assert.AreEqual(17L, detail.ContractId); Assert.AreEqual(4, detail.StandardId); Assert.AreEqual("Tea", detail.StandardName);
        Assert.AreEqual(12.5m, detail.OrderQty); Assert.AreSame(version, detail.RowVersion); Assert.AreEqual("operator", detail.User);
        var standard = service.GetAvailableStandardsById(17).Single();
        Assert.AreEqual(17, standard.ContractId); Assert.AreEqual(4, standard.StandardId); Assert.AreEqual("Tea", standard.StandardName); Assert.AreEqual("0", standard.OrderQty);
        Assert.AreEqual("Tea", service.GetAvailableStandardsByIdAndSearchStr(17, "all", 2).Single().StandardName);
        var blend = service.GetBlendListByConAndStanList(17, 4).Single();
        Assert.AreEqual("B1", blend.BlendNo); Assert.AreEqual("Open", blend.ClosedStatus);
    }

    [TestMethod]
    public void WritesMapInputsAndReportFailure()
    {
        var version = new byte[] { 1, 2 };
        var data = new FakeData();
        var service = new ContractService(data);
        var contract = new BuyerContract { ContractId = 17, CompanyId = 2, ContractName = "C", BuyerId = 3, CompletionDate = "2026-11-17", User = "operator", RowVersion = version };
        var detail = new BuyerContractDetail { ContractId = 17, StandardId = 4, OrderQty = 12.5m, User = "operator", RowVersion = version };
        Assert.IsTrue(service.CreateContract(contract).Succeeded);
        Assert.AreEqual(2, data.Contract!.CompanyId); Assert.AreEqual(3, data.Contract.BuyerId); Assert.AreEqual("C", data.Contract.ContractName);
        Assert.AreEqual("2026-11-17", data.Contract.CompletionDate); Assert.AreEqual("operator", data.Contract.User); Assert.AreSame(version, data.Contract.RowVersion);
        Assert.IsTrue(service.UpdateContract(contract).Succeeded);
        Assert.IsTrue(service.DeleteContract(contract).Succeeded); Assert.AreEqual(17L, data.ContractId);
        Assert.IsTrue(service.CreateBuyerContractDetail(detail).Succeeded);
        Assert.AreEqual(17L, data.Detail!.ContractId); Assert.AreEqual(4, data.Detail.StandardId); Assert.AreEqual(12.5m, data.Detail.OrderQty);
        Assert.AreSame(version, data.Detail.RowVersion); Assert.AreEqual("operator", data.Detail.User);
        Assert.IsTrue(service.UpdateBuyerContractDetail(detail).Succeeded);
        Assert.IsTrue(service.DeleteBuyerContractDetail(detail).Succeeded);
        data.Result = false;
        Assert.IsFalse(service.CreateContract(contract).Succeeded); Assert.IsFalse(service.UpdateContract(contract).Succeeded);
        Assert.IsFalse(service.DeleteContract(contract).Succeeded); Assert.IsFalse(service.CreateBuyerContractDetail(detail).Succeeded);
        Assert.IsFalse(service.UpdateBuyerContractDetail(detail).Succeeded); Assert.IsFalse(service.DeleteBuyerContractDetail(detail).Succeeded);
    }

    [TestMethod]
    public void NullsAndUnsupportedLegacyLookupFailExplicitly()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ContractService(null!));
        var service = new ContractService(new FakeData());
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateContract(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateContract(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.DeleteContract(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateBuyerContractDetail(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateBuyerContractDetail(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.DeleteBuyerContractDetail(null!));
        Assert.ThrowsExactly<NotImplementedException>(() => service.GetContractById(1));
    }

    private sealed class FakeData : IContractDataService
    {
        public IReadOnlyList<ContractRecord> Contracts { get; init; } = [];
        public IReadOnlyList<ContractDetailRecord> Details { get; init; } = [];
        public IReadOnlyList<AvailableStandardRecord> Standards { get; init; } = [];
        public IReadOnlyList<ContractBlendRecord> Blends { get; init; } = [];
        public (string, int) Search { get; private set; }
        public (long, string, int) StandardSearch { get; private set; }
        public int CompanyId { get; private set; }
        public long ContractId { get; private set; }
        public ContractRecord? Contract { get; private set; }
        public ContractDetailRecord? Detail { get; private set; }
        public bool Result { get; set; } = true;
        public IReadOnlyList<ContractRecord> GetContracts(string searchText, int companyId) { Search = (searchText, companyId); return Contracts; }
        public IReadOnlyList<ContractRecord> GetOpenContracts(int companyId) { CompanyId = companyId; return Contracts; }
        public bool CreateContract(ContractRecord contract) { Contract = contract; return Result; }
        public bool UpdateContract(ContractRecord contract) { Contract = contract; return Result; }
        public bool DeleteContract(long contractId) { ContractId = contractId; return Result; }
        public IReadOnlyList<ContractDetailRecord> GetContractDetails(long contractId) { ContractId = contractId; return Details; }
        public IReadOnlyList<AvailableStandardRecord> GetAvailableStandards(long contractId) { ContractId = contractId; return Standards; }
        public IReadOnlyList<AvailableStandardRecord> SearchAvailableStandards(long contractId, string searchText, int companyId) { StandardSearch = (contractId, searchText, companyId); return Standards; }
        public bool CreateContractDetail(ContractDetailRecord detail) { Detail = detail; return Result; }
        public bool UpdateContractDetail(ContractDetailRecord detail) { Detail = detail; return Result; }
        public bool DeleteContractDetail(ContractDetailRecord detail) { Detail = detail; return Result; }
        public IReadOnlyList<ContractBlendRecord> GetBlends(int contractId, int standardId) => Blends;
    }
}
