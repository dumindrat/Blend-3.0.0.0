using Blend.DataService.Contracts;

namespace Blend.BusinessService.Contracts;

public sealed class ContractService : IContractService
{
    private readonly IContractDataService _dataService;

    public ContractService(IContractDataService dataService) =>
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public IReadOnlyList<BuyerContract> GetContractList(string searchText, int companyId)
    {
        ArgumentNullException.ThrowIfNull(searchText);
        var search = searchText == "all" ? string.Empty : searchText.Replace('~', '/');
        return _dataService.GetContracts(search, companyId).Select(record => ToContract(record, companyId)).ToArray();
    }

    public IReadOnlyList<BuyerContract> GetOpenContractList(int companyId) =>
        _dataService.GetOpenContracts(companyId).Select(record => ToContract(record, companyId)).ToArray();

    public BusinessOperationResult CreateContract(BuyerContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return Result(_dataService.CreateContract(ToRecord(contract)), "The contract could not be created.");
    }

    public BusinessOperationResult UpdateContract(BuyerContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return Result(_dataService.UpdateContract(ToRecord(contract)), "The contract could not be updated.");
    }

    public BusinessOperationResult DeleteContract(BuyerContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return Result(_dataService.DeleteContract(contract.ContractId), "The contract could not be deleted.");
    }

    public BuyerContract GetContractById(int id) =>
        throw new NotImplementedException("The legacy contract-by-ID lookup is not implemented.");

    public IReadOnlyList<BuyerContractDetail> GetBuyerContractDetailById(long contractId) =>
        _dataService.GetContractDetails(contractId).Select(ToDetail).ToArray();

    public IReadOnlyList<TeaStandardForContract> GetAvailableStandardsById(long contractId) =>
        _dataService.GetAvailableStandards(contractId).Select(ToStandard).ToArray();

    public IReadOnlyList<TeaStandardForContract> GetAvailableStandardsByIdAndSearchStr(long contractId, string searchText, int companyId)
    {
        ArgumentNullException.ThrowIfNull(searchText);
        return _dataService.SearchAvailableStandards(contractId, searchText == "all" ? string.Empty : searchText, companyId)
            .Select(ToStandard).ToArray();
    }

    public BusinessOperationResult CreateBuyerContractDetail(BuyerContractDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        return Result(_dataService.CreateContractDetail(ToRecord(detail)), "The contract detail could not be created.");
    }

    public BusinessOperationResult UpdateBuyerContractDetail(BuyerContractDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        return Result(_dataService.UpdateContractDetail(ToRecord(detail)), "The contract detail could not be updated.");
    }

    public BusinessOperationResult DeleteBuyerContractDetail(BuyerContractDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        return Result(_dataService.DeleteContractDetail(ToRecord(detail)), "The contract detail could not be deleted.");
    }

    public IReadOnlyList<BlendNoByCon> GetBlendListByConAndStanList(int contractId, int standardId) =>
        _dataService.GetBlends(contractId, standardId).Select(item => new BlendNoByCon { BlendNo = item.BlendNo, ClosedStatus = item.ClosedStatus }).ToArray();

    private static BusinessOperationResult Result(bool succeeded, string message) =>
        succeeded ? BusinessOperationResult.Success() : BusinessOperationResult.Failure(message);

    private static BuyerContract ToContract(ContractRecord record, int companyId) => new()
    {
        ContractId = record.ContractId, CompanyId = companyId, ContractName = record.ContractName, Contract = record.Contract,
        BuyerId = record.BuyerId, BuyerCode = record.BuyerCode, BuyerName = record.BuyerName, CompletionDate = record.CompletionDate,
        ModifiedDate = record.ModifiedDate, IsClosed = record.IsClosed, CloseStatus = record.CloseStatus, RowVersion = record.RowVersion, User = record.User
    };

    private static ContractRecord ToRecord(BuyerContract contract) => new()
    {
        ContractId = contract.ContractId, CompanyId = contract.CompanyId, ContractName = contract.ContractName, Contract = contract.Contract,
        BuyerId = contract.BuyerId, BuyerCode = contract.BuyerCode, BuyerName = contract.BuyerName, CompletionDate = contract.CompletionDate,
        ModifiedDate = contract.ModifiedDate, IsClosed = contract.IsClosed, CloseStatus = contract.CloseStatus, RowVersion = contract.RowVersion, User = contract.User
    };

    private static BuyerContractDetail ToDetail(ContractDetailRecord record) => new()
    {
        ContractId = record.ContractId, StandardId = record.StandardId, StandardName = record.StandardName,
        OrderQty = record.OrderQty, RowVersion = record.RowVersion, User = record.User
    };

    private static ContractDetailRecord ToRecord(BuyerContractDetail detail) => new()
    {
        ContractId = detail.ContractId, StandardId = detail.StandardId, StandardName = detail.StandardName,
        OrderQty = detail.OrderQty, RowVersion = detail.RowVersion, User = detail.User
    };

    private static TeaStandardForContract ToStandard(AvailableStandardRecord record) => new()
    {
        ContractId = record.ContractId, StandardId = record.StandardId, StandardName = record.StandardName, OrderQty = record.OrderQty
    };
}
