using Blend.BuisnessEntities.AuctionBrokers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Broker
{
    public class BrokerService : BaseManagement<TeaBroker>, IBrokerService
    {
        public TeaBroker GetBrokerById(int BrokerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TeaBroker> GetAllBrokers()
        {
            var mapper = new DataMapper<TeaBroker>("GetBrokers", new Hashtable() { });
            var result = mapper.ObjectList();
            return result;
        }

        public bool CreateBroker(TeaBroker BrokerEntity)
        {
            dataOperation.CommandText("trn_BrokerInsert");
            dataOperation.AddParam("@BCode", BrokerEntity.BrokerCode);
            dataOperation.AddParam("@BName", BrokerEntity.BrokerName);
            dataOperation.AddParam("@BAddress", BrokerEntity.Address);
            dataOperation.AddParam("@BEmail", BrokerEntity.Email);
            dataOperation.AddParam("@BTel", BrokerEntity.Tel);
            dataOperation.AddParam("@BContactPerson", BrokerEntity.ContactPerson);
            dataOperation.AddParam("@User", BrokerEntity.User);

            dataOperation.DoExecute();
            return true;
        }

        public bool UpdateBroker(TeaBroker BrokerEntity)
        {
            dataOperation.CommandText("trn_BrokerUpdate");
            dataOperation.AddParam("@BId", BrokerEntity.BrokerId);
            dataOperation.AddParam("@BCode", BrokerEntity.BrokerCode);
            dataOperation.AddParam("@BName", BrokerEntity.BrokerName);
            dataOperation.AddParam("@BAddress", BrokerEntity.Address);
            dataOperation.AddParam("@BEmail", BrokerEntity.Email);
            dataOperation.AddParam("@BTel", BrokerEntity.Tel);
            dataOperation.AddParam("@BContactPerson", BrokerEntity.ContactPerson);
            dataOperation.AddParam("@RowVersion", BrokerEntity.RowVersion);
            dataOperation.AddParam("@User", BrokerEntity.User);

            dataOperation.DoExecute();
            return true;

        }

        public bool DeleteBroker(int BrokerId)
        {
            dataOperation.CommandText("trn_BrokerDelete");
            dataOperation.AddParam("@BId", BrokerId);
            dataOperation.DoExecute();
            return true;
        }
    }
}
