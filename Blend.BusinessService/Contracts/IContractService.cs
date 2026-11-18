namespace Blend.BusinessService.Contracts;

public interface IContractService
{
    IReadOnlyList<BuyerContract> GetContractList(string searchText, int companyId);
    IReadOnlyList<BuyerContract> GetOpenContractList(int companyId);
    BusinessOperationResult CreateContract(BuyerContract contract);
    BusinessOperationResult UpdateContract(BuyerContract contract);
    BusinessOperationResult DeleteContract(BuyerContract contract);
    BuyerContract GetContractById(int id);
    IReadOnlyList<BuyerContractDetail> GetBuyerContractDetailById(long contractId);
    IReadOnlyList<TeaStandardForContract> GetAvailableStandardsById(long contractId);
    IReadOnlyList<TeaStandardForContract> GetAvailableStandardsByIdAndSearchStr(long contractId, string searchText, int companyId);
    BusinessOperationResult CreateBuyerContractDetail(BuyerContractDetail detail);
    BusinessOperationResult UpdateBuyerContractDetail(BuyerContractDetail detail);
    BusinessOperationResult DeleteBuyerContractDetail(BuyerContractDetail detail);
    IReadOnlyList<BlendNoByCon> GetBlendListByConAndStanList(int contractId, int standardId);
}
