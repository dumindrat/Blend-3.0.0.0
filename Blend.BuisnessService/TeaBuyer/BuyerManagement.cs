using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using Blend.BuisnessEntities.TeaBuyer;

namespace Blend.BuisnessService.TeaBuyer
{
    public class BuyerManagement : BaseManagement<Buyer>, IBuyerService
    {

        public IList<Buyer> GetDataList(long key)
        {
            var mapper = new DataMapper<Buyer>("GetBuyers", new Hashtable() { });

            var result = mapper.ObjectList();
            return result;
        }

        public Buyer GetBuyerById(int buyerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Buyer> GetAllBuyers(string serchStr, int companyId)
        {
            var mapper = new DataMapper<Buyer>("GetBuyers", new Hashtable() { { "CompanyId", companyId }, { "searchstring", serchStr } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool CreateBuyer(Buyer buyerEntity)
        {
            dataOperation.CommandText("trn_BuyerInsert");
            dataOperation.AddParam("@BuyerCode", buyerEntity.BuyerCode);
            dataOperation.AddParam("@BuyerName", buyerEntity.BuyerName);
            dataOperation.AddParam("@Address", buyerEntity.Address);
            dataOperation.AddParam("@Email", buyerEntity.Email);
            dataOperation.AddParam("@User", buyerEntity.User);
            dataOperation.AddParam("@CompanyId", buyerEntity.CompanyId);
            dataOperation.DoExecute();
            return true;


        }

        public bool UpdateBuyer(int buyerId, Buyer buyerEntity)
        {
            dataOperation.CommandText("trn_BuyerUpdate");
            dataOperation.AddParam("@BuyerCode", buyerEntity.BuyerCode);
            dataOperation.AddParam("@BId", buyerEntity.BuyerId);
            dataOperation.AddParam("@TeaBuyerName", buyerEntity.BuyerName);
            dataOperation.AddParam("@BAddress", buyerEntity.Address);
            dataOperation.AddParam("@BEmail", buyerEntity.Email);
            dataOperation.AddParam("@BRowVersion", buyerEntity.RowVersion);
            dataOperation.DoExecute();
            return true;

        }

        public bool DeleteBuyer(int buyerId)
        {
            dataOperation.CommandText("trn_BuyerDelete");
            dataOperation.AddParam("@Ids", buyerId);
            dataOperation.DoExecute();
            return true;

        }


        IList<Buyer> IBuyerService.GetAllBuyers(string serchStr, int companyId)
        {
            try
            {
                var searchBy = serchStr == "all" ? "" : serchStr;
                //var mapper = new DataMapper<Buyer>("GetBuyers", new Hashtable() { { "Name", searchBy } });
                var mapper = new DataMapper<Buyer>("GetBuyers", new Hashtable() { { "CompanyId", companyId }, { "Name", searchBy } });


                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("Create Buyer fails. Error " + ex.Message);
                throw ex;
            }

        }
    }
}
