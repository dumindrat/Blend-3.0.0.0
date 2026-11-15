namespace Blend.BusinessService.Brokers;

public interface IBrokerService
{
    Broker? GetBrokerById(int brokerId);
    IReadOnlyList<Broker> GetAllBrokers(string searchText, int companyId);
    BusinessOperationResult CreateBroker(Broker broker);
    BusinessOperationResult UpdateBroker(Broker broker);
    BusinessOperationResult DeleteBroker(int brokerId);
}
