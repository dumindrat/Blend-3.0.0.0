using System.Collections.Generic;

namespace Blend.DataService.Contracts;

public interface IContractDataService
{
    IReadOnlyList<ContractRecord> GetContracts(string searchText, int companyId);
    IReadOnlyList<ContractRecord> GetOpenContracts(int companyId);
    bool CreateContract(ContractRecord contract);
    bool UpdateContract(ContractRecord contract);
    bool DeleteContract(long contractId);
    IReadOnlyList<ContractDetailRecord> GetContractDetails(long contractId);
    IReadOnlyList<AvailableStandardRecord> GetAvailableStandards(long contractId);
    IReadOnlyList<AvailableStandardRecord> SearchAvailableStandards(long contractId, string searchText, int companyId);
    bool CreateContractDetail(ContractDetailRecord detail);
    bool UpdateContractDetail(ContractDetailRecord detail);
    bool DeleteContractDetail(ContractDetailRecord detail);
    IReadOnlyList<ContractBlendRecord> GetBlends(int contractId, int standardId);
}
