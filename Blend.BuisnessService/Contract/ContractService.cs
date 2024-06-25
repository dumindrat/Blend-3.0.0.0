using Blend.BuisnessEntities.Contract;
using Blend.BuisnessEntities.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Contract
{
    public class ContractService : BaseManagement<BuyerContract>, IContractService
    {
        public IEnumerable<BuyerContract> GetContractList(string serchStr, int companyId)
        {
            try
            {
                var searchBy = serchStr == "all" ? "" : serchStr.Replace('~', '/');
                var mapper = new DataMapper<BuyerContract>("GetContracts", new Hashtable() { { "ContractName", searchBy }, { "CompanyId", companyId } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetContractList fails. Error " + ex.Message);
                throw ex;
            }

        }

        public IEnumerable<BuyerContract> GetOpenContractList()
        {
            try
            {
                var mapper = new DataMapper<BuyerContract>("GetOpenContracts", new Hashtable() { });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetContractList fails. Error " + ex.Message);
                throw ex;
            }
            finally
            {
                dataOperation.ConnectionClose();
            }
        }

        public bool CreateContract(BuyerContract obj)
        {
            dataOperation.CommandText("trn_ContractHeaderInsert");
            dataOperation.AddParam("@ContractName", obj.ContractName);
            dataOperation.AddParam("@BuyerId", obj.BuyerId);
            dataOperation.AddParam("@CompletionDate", obj.CompletionDate);
            dataOperation.AddParam("@CreateUser", obj.User);
            dataOperation.AddParam("@CompanyId", obj.CompanyId);
            dataOperation.DoExecute();
            return true;
        }

        public bool UpdateContract(BuyerContract obj)
        {
            dataOperation.CommandText("trn_ContractHeaderUpdate");
            dataOperation.AddParam("Id", obj.ContractId);
            dataOperation.AddParam("ContractName", obj.ContractName);
            dataOperation.AddParam("CompletionDate", obj.CompletionDate);
            dataOperation.AddParam("ModifiedUser", obj.User);
            dataOperation.AddParam("RowVersion", obj.RowVersion);
            dataOperation.DoExecute();
            return true;

        }

        public bool DeleteContract(BuyerContract obj)
        {
            dataOperation.CommandText("trn_ContractHeaderDelete");
            dataOperation.AddParam("Id", obj.ContractId);
            dataOperation.DoExecute();
            return true;
        }

        public BuyerContract GetContractById(int id)
        {
            var obj = new BuyerContract { ContractId = id, OperationCommand = "GetContractById" };
            return null;
        }

        public IEnumerable<BuyerContractDetail> GetBuyerContractDetailById(long key)
        {
            try
            {
                var ob = new BuyerContract();
                ob.ContractId = key;
                var mapper = new DataMapper<BuyerContractDetail>("GetBuyerContractDetailById", ob);

                return mapper.ObjectList();
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetBuyerContractDetailById fails. Error " + ex.Message);
                throw ex;
            }


        }

        public IList<TeaStandardForContract> GetAvailableStandardsById(long key)
        {

            try
            {
                var mapper = new DataMapper<TeaStandardForContract>("GetAvailableStandardsById", new Hashtable() { { "ContractId", key } });

                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetAvailableStandardsById fails. Error " + ex.Message);
                throw ex;
            }

        }

        public bool CreateBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailInsert");
            dataOperation.AddParam("@ContractId", obj.ContractId);
            dataOperation.AddParam("@StandardId", obj.StandardId);
            dataOperation.AddParam("@ExportQty", obj.OrderQty);
            dataOperation.AddParam("@CreateUser", obj.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool UpdateBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailUpdate");
            dataOperation.AddParam("@CId", obj.ContractId);
            dataOperation.AddParam("@SId", obj.StandardId);
            dataOperation.AddParam("@ExpQty", obj.OrderQty);
            dataOperation.AddParam("@RowVersion", obj.RowVersion);
            dataOperation.AddParam("@User", obj.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool DeleteBuyerContractDetail(BuyerContractDetail obj)
        {
            dataOperation.CommandText("trn_ContractDetailDelete");
            dataOperation.AddParam("@ContractId", obj.ContractId);
            dataOperation.AddParam("@StandardId", obj.StandardId);
            dataOperation.AddParam("@RowVersion", obj.RowVersion);
            dataOperation.DoExecute();
            return true;
        }





        public IList<TeaStandardForContract> GetAvailableStandardsByIdAndSearchStr(long key, string searchstr, int companyId)
        {
            try
            {
                var searchBy = searchstr == "all" ? "" : searchstr;
                var mapper = new DataMapper<TeaStandardForContract>("GetAvailableStandardsByIdAndSerchKey", new Hashtable() { 
                { "ContractId", key }, 
                { "StandardName", searchBy },
                { "CompanyId", companyId } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetAvailableStandardsByIdAndSearchKey fails. Error " + ex.Message);
                throw ex;
            }
        }


        public IEnumerable<BlendNoByCon> GetBlendListByConAndStanList(int contractId, int standardId)
        {
            var mapper = new DataMapper<BlendNoByCon>("GetBlendNoByContractAndStandard", new Hashtable() { { "ContractId", contractId }, { "StandardId", standardId } });
            var result = mapper.ObjectList();
            return result;
        }
    }
}
