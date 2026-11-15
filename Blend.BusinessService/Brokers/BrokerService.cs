using Blend.DataService.Brokers;

namespace Blend.BusinessService.Brokers;

public sealed class BrokerService : IBrokerService
{
    private readonly IBrokerDataService _brokerDataService;

    public BrokerService(IBrokerDataService brokerDataService)
    {
        _brokerDataService = brokerDataService ?? throw new ArgumentNullException(nameof(brokerDataService));
    }

    public Broker? GetBrokerById(int brokerId)
    {
        var record = _brokerDataService.GetBrokerById(brokerId);
        return record is null ? null : ToBroker(record);
    }

    public IReadOnlyList<Broker> GetAllBrokers(string searchText, int companyId) =>
        _brokerDataService.GetAllBrokers(searchText, companyId).Select(ToBroker).ToArray();

    public BusinessOperationResult CreateBroker(Broker broker)
    {
        ArgumentNullException.ThrowIfNull(broker);
        return ToOperationResult(
            _brokerDataService.CreateBroker(ToRecord(broker)),
            "The broker could not be created.");
    }

    public BusinessOperationResult UpdateBroker(Broker broker)
    {
        ArgumentNullException.ThrowIfNull(broker);
        return ToOperationResult(
            _brokerDataService.UpdateBroker(ToRecord(broker)),
            "The broker could not be updated.");
    }

    public BusinessOperationResult DeleteBroker(int brokerId) => ToOperationResult(
        _brokerDataService.DeleteBroker(brokerId),
        "The broker could not be deleted.");

    private static BusinessOperationResult ToOperationResult(bool succeeded, string failureMessage) =>
        succeeded ? BusinessOperationResult.Success() : BusinessOperationResult.Failure(failureMessage);

    private static Broker ToBroker(BrokerRecord record) => new()
    {
        BrokerId = record.BrokerId,
        CompanyId = record.CompanyId,
        BrokerCode = record.BrokerCode,
        BrokerName = record.BrokerName,
        Address = record.Address,
        Email = record.Email,
        Tel = record.Tel,
        ContactPerson = record.ContactPerson,
        ModifiedDate = record.ModifiedDate,
        RowVersion = record.RowVersion,
        User = record.User
    };

    private static BrokerRecord ToRecord(Broker broker) => new()
    {
        BrokerId = broker.BrokerId,
        CompanyId = broker.CompanyId,
        BrokerCode = broker.BrokerCode,
        BrokerName = broker.BrokerName,
        Address = broker.Address,
        Email = broker.Email,
        Tel = broker.Tel,
        ContactPerson = broker.ContactPerson,
        ModifiedDate = broker.ModifiedDate,
        RowVersion = broker.RowVersion,
        User = broker.User
    };
}
