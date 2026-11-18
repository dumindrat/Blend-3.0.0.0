using System;
using System.Collections.Generic;
using Blend.DataService.Mapping;

namespace Blend.DataService.Contracts;

public sealed class ContractDataService : IContractDataService
{
    private readonly IStoredProcedureMapper _mapper;

    public ContractDataService(IStoredProcedureMapper mapper) =>
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public IReadOnlyList<ContractRecord> GetContracts(string searchText, int companyId) =>
        _mapper.Query<ContractRecord>("trn_ContractHeaderGet", new { searchstr = searchText, CompanyId = companyId });

    public IReadOnlyList<ContractRecord> GetOpenContracts(int companyId) =>
        _mapper.Query<ContractRecord>("trn_ContractOpenGet", new { CompanyId = companyId });

    public bool CreateContract(ContractRecord contract)
    {
        if (contract is null) throw new ArgumentNullException(nameof(contract));
        _mapper.Execute("trn_ContractHeaderInsert", new
        {
            contract.ContractName, contract.BuyerId, contract.CompletionDate, CreateUser = contract.User, contract.CompanyId
        });
        return true;
    }

    public bool UpdateContract(ContractRecord contract)
    {
        if (contract is null) throw new ArgumentNullException(nameof(contract));
        _mapper.Execute("trn_ContractHeaderUpdate", new
        {
            Id = contract.ContractId, contract.ContractName, contract.CompletionDate, ModifiedUser = contract.User, contract.RowVersion
        });
        return true;
    }

    public bool DeleteContract(long contractId)
    {
        _mapper.Execute("trn_ContractHeaderDelete", new { Id = contractId });
        return true;
    }

    public IReadOnlyList<ContractDetailRecord> GetContractDetails(long contractId) =>
        _mapper.Query<ContractDetailRecord>("trn_ContractDetailGetByContractId", new { Id = contractId });

    public IReadOnlyList<AvailableStandardRecord> GetAvailableStandards(long contractId) =>
        _mapper.Query<AvailableStandardRecord>("trn_ContractAvailableStandards", new { Id = contractId });

    public IReadOnlyList<AvailableStandardRecord> SearchAvailableStandards(long contractId, string searchText, int companyId) =>
        _mapper.Query<AvailableStandardRecord>("trn_ContractAvailableStandardsBySearch", new { Id = contractId, searchkey = searchText, CompanyId = companyId });

    public bool CreateContractDetail(ContractDetailRecord detail)
    {
        if (detail is null) throw new ArgumentNullException(nameof(detail));
        _mapper.Execute("trn_ContractDetailInsert", new
        {
            detail.ContractId, detail.StandardId, ExportQty = detail.OrderQty, CreateUser = detail.User
        });
        return true;
    }

    public bool UpdateContractDetail(ContractDetailRecord detail)
    {
        if (detail is null) throw new ArgumentNullException(nameof(detail));
        _mapper.Execute("trn_ContractDetailUpdate", new
        {
            CId = detail.ContractId, SId = detail.StandardId, ExpQty = detail.OrderQty, detail.RowVersion, detail.User
        });
        return true;
    }

    public bool DeleteContractDetail(ContractDetailRecord detail)
    {
        if (detail is null) throw new ArgumentNullException(nameof(detail));
        _mapper.Execute("trn_ContractDetailDelete", new { detail.ContractId, detail.StandardId, detail.RowVersion });
        return true;
    }

    public IReadOnlyList<ContractBlendRecord> GetBlends(int contractId, int standardId) =>
        _mapper.Query<ContractBlendRecord>("trn_BlendNoGetByContractAndStandard", new { ContractId = contractId, StandardId = standardId });
}
