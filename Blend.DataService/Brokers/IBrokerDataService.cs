using System.Collections.Generic;

namespace Blend.DataService.Brokers
{
    public interface IBrokerDataService
    {
        BrokerRecord? GetBrokerById(int brokerId);
        IReadOnlyList<BrokerRecord> GetAllBrokers(string searchText, int companyId);
        bool CreateBroker(BrokerRecord broker);
        bool UpdateBroker(BrokerRecord broker);
        bool DeleteBroker(int brokerId);
    }
}
