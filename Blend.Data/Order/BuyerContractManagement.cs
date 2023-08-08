using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blend.Data.Order
{
    public class BuyerContractManagement : BaseManagement<BuyerContract>, IOperation<BuyerContract>
    {
        public void Create(BuyerContract obj)
        {
            dataOperation.CommandText("trn_ContractHeaderInsert");
            dataOperation.AddParam("ContractName", obj.ContractName);
            dataOperation.AddParam("BuyerId", obj.BuyerId);
            dataOperation.AddParam("CompletionDate", obj.CompletionDate);
            dataOperation.AddParam("CreateUser", "user@123");
            dataOperation.DoExecute();
        }

        public void Update(BuyerContract obj)
        {
            dataOperation.CommandText("trn_ContractHeaderUpdate");
            dataOperation.AddParam("Id", obj.ContractId);
            dataOperation.AddParam("ContractName", obj.ContractName);
            dataOperation.AddParam("CompletionDate", obj.CompletionDate);
            dataOperation.AddParam("ModifiedUser", "user@123");
            dataOperation.DoExecute();
        }

        public BuyerContract GetDataById(int key)
        {
            var obj = new BuyerContract { ContractId = key, OperationCommand = "GetContractById" };
            return BaseRepo.Retrieve(obj);
        }

        public void Delete(BuyerContract obj)
        {
            obj.OperationCommand = "DeleteBuyerContract";
            BaseRepo.Delete(obj);
        }

        public List<BuyerContractDetail> GetBuyerContractDetailById(long key)
        {
            var ob = new BuyerContract();
            ob.ContractId = key;
            return Mapper.QueryForList<BuyerContractDetail>("GetBuyerContractDetailById", ob).ToList();
        }

        public List<Blend.Data.Standard.TeaStandard> GetAvailableStandardsById(long key)
        {
            var ob = new BuyerContract();
            ob.ContractId = key;
            return Mapper.QueryForList<Blend.Data.Standard.TeaStandard>("GetAvailableStandardsById", ob).ToList();
        }

        public void CreateBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailInsert");
            dataOperation.AddParam("ContractId", obj.ContractId);
            dataOperation.AddParam("StandardId", obj.StandardId);
            dataOperation.AddParam("ExportQty", obj.ExportQty);
            dataOperation.AddParam("CreateUser", "user@123");
            dataOperation.DoExecute();
        }

        public void UpdateBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailUpdate");
            dataOperation.AddParam("CId", obj.ContractId);
            dataOperation.AddParam("SId", obj.StandardId);
            dataOperation.AddParam("ExpQty", obj.ExportQty);
            dataOperation.AddParam("RowVersion", obj.RowVersion);
            dataOperation.AddParam("User", "user@123");

            dataOperation.DoExecute();
        }

        public void DeleteBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailDelete");
            dataOperation.AddParam("ContractId", obj.ContractId);
            dataOperation.AddParam("StandardId", obj.StandardId);
            dataOperation.AddParam("RowVersion", obj.RowVersion);
            dataOperation.DoExecute();
        }

        public IList<BuyerContract> GetDataList(long key)
        {
            var result = Mapper.QueryForList<BuyerContract>("GetContracts", new Hashtable() { });
            return result;
        }


        public BuyerContract GetDataById(BuyerContract obj)
        {
            throw new NotImplementedException();
        }
    }
}
