using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace Blend.Data.TeaBuyer
{
    public class BuyerManagement : BaseManagement<Buyer>, IOperation<Buyer>
    {
        public void Create(Buyer obj)
        {
            dataOperation.CommandText("trn_BuyerInsert");
            dataOperation.AddParam("@BuyerName", obj.BuyerName);
            dataOperation.AddParam("@Address", obj.Address);
            dataOperation.AddParam("@Email", obj.Email);
            dataOperation.DoExecute();
        }

        public void Update(Buyer obj)
        {
            dataOperation.CommandText("trn_BuyerUpdate");
            dataOperation.AddParam("Id", obj.BuyerId);
            dataOperation.AddParam("BuyerName", obj.BuyerName);
            dataOperation.AddParam("Address", obj.Address);
            dataOperation.AddParam("Email", obj.Email);
            dataOperation.DoExecute();
        }

        public void Delete(Buyer obj)
        {
            dataOperation.CommandText("trn_BuyerDelete");
            dataOperation.AddParam("Id", obj.BuyerId);
            dataOperation.DoExecute();
        }

        public Buyer GetDataById(Buyer obj)
        {
            throw new NotImplementedException();
        }

        public IList<Buyer> GetDataList(long key)
        {
            var result = Mapper.QueryForList<Buyer>("GetBuyers", new Hashtable() { });
            return result;
        }
    }
}
