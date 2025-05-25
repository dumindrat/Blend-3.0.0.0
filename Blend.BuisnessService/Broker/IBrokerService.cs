using Blend.BuisnessEntities.AuctionBrokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Broker
{
    public interface IBrokerService
    {
        TeaBroker GetBrokerById(int BrokerId);
        IEnumerable<TeaBroker> GetAllBrokers(string searchstr, int companyId);
        bool CreateBroker(TeaBroker BrokerEntity);
        bool UpdateBroker(TeaBroker BrokerEntity);
        bool DeleteBroker(int BrokerId);
    }
}
