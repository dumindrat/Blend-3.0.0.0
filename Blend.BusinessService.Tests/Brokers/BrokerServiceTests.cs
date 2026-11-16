using Blend.BusinessService.Brokers;
using Blend.DataService.Brokers;

namespace Blend.BusinessService.Tests.Brokers;

[TestClass]
public sealed class BrokerServiceTests
{
    [TestMethod]
    public void GetBrokerById_MapsRecordToBusinessModel()
    {
        var record = new BrokerRecord
        {
            BrokerId = 42,
            CompanyId = 7,
            BrokerCode = "BRK",
            BrokerName = "Example Broker",
            Address = "Address",
            Email = "broker@example.com",
            Tel = "123456",
            ContactPerson = "Contact",
            ModifiedDate = "2026-11-15",
            RowVersion = [1, 2, 3],
            User = "operator"
        };
        var dataService = new FakeBrokerDataService { Broker = record };
        var service = new BrokerService(dataService);

        var result = service.GetBrokerById(42);

        Assert.IsNotNull(result);
        Assert.AreEqual(42, dataService.RequestedBrokerId);
        AssertBrokerMatchesRecord(result, record);
    }

    [TestMethod]
    public void GetBrokerById_ReturnsNullWhenRecordDoesNotExist()
    {
        var service = new BrokerService(new FakeBrokerDataService());

        Assert.IsNull(service.GetBrokerById(999));
    }

    [TestMethod]
    public void CreateBroker_ReturnsFailureResultWhenDataServiceReportsFailure()
    {
        var service = new BrokerService(new FakeBrokerDataService { CreateResult = false });

        var result = service.CreateBroker(new Broker { BrokerName = "Example Broker" });

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("The broker could not be created.", result.ErrorMessage);
    }

    [TestMethod]
    public void CreateBroker_ReturnsSuccessResultWhenDataServiceSucceeds()
    {
        var service = new BrokerService(new FakeBrokerDataService { CreateResult = true });

        var result = service.CreateBroker(new Broker { BrokerName = "Example Broker" });

        Assert.IsTrue(result.Succeeded);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void GetBrokerById_PropagatesUnexpectedDataServiceException()
    {
        var expected = new InvalidOperationException("Database failure.");
        var service = new BrokerService(new FakeBrokerDataService { Exception = expected });

        var actual = Assert.ThrowsExactly<InvalidOperationException>(() => service.GetBrokerById(42));

        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public void CreateBroker_PropagatesUnexpectedDataServiceException()
    {
        var expected = new InvalidOperationException("Database failure.");
        var service = new BrokerService(new FakeBrokerDataService { Exception = expected });

        var actual = Assert.ThrowsExactly<InvalidOperationException>(
            () => service.CreateBroker(new Broker { BrokerName = "Example Broker" }));

        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public void CreateBroker_RejectsNullBroker()
    {
        var service = new BrokerService(new FakeBrokerDataService());

        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateBroker(null!));
    }

    [TestMethod]
    public void UpdateBroker_WhenDataServiceReportsFailure_ReturnsFailure()
    {
        var service = new BrokerService(new FakeBrokerDataService { CreateResult = false });

        var result = service.UpdateBroker(new Broker { BrokerId = 8, CompanyId = 2, RowVersion = [1] });

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("The broker could not be updated.", result.ErrorMessage);
    }

    [TestMethod]
    public void DeleteBroker_WhenDataServiceReportsFailure_ReturnsFailure()
    {
        var service = new BrokerService(new FakeBrokerDataService { CreateResult = false });

        var result = service.DeleteBroker(8);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("The broker could not be deleted.", result.ErrorMessage);
    }

    [TestMethod]
    public void GetAllBrokers_MapsAllRecordsAndPassesSearchAndCompany()
    {
        var records = new[]
        {
            new BrokerRecord { BrokerId = 1, CompanyId = 2, BrokerName = "North", Address = "First", RowVersion = [1] },
            new BrokerRecord { BrokerId = 2, CompanyId = 2, BrokerCode = "ABC", ContactPerson = "Contact", RowVersion = [2] }
        };
        var dataService = new FakeBrokerDataService { Brokers = records };
        var service = new BrokerService(dataService);

        var result = service.GetAllBrokers("North", 2);

        Assert.AreEqual("North", dataService.SearchText);
        Assert.AreEqual(2, dataService.SearchCompanyId);
        Assert.AreEqual(2, result.Count);
        AssertBrokerMatchesRecord(result[0], records[0]);
        AssertBrokerMatchesRecord(result[1], records[1]);
    }

    [TestMethod]
    public void CreateAndUpdateBroker_MapAllFieldsAndReturnSuccess()
    {
        var dataService = new FakeBrokerDataService();
        var service = new BrokerService(dataService);
        var broker = new Broker
        {
            BrokerId = 42, CompanyId = 2, BrokerCode = "ABC", BrokerName = "Name",
            Address = "Address", Email = "broker@example.com", Tel = "123456",
            ContactPerson = "Contact", ModifiedDate = "2026-11-15", RowVersion = [1, 2], User = "operator"
        };

        var created = service.CreateBroker(broker);
        var updated = service.UpdateBroker(broker);

        Assert.IsTrue(created.Succeeded);
        Assert.IsNull(created.ErrorMessage);
        Assert.IsTrue(updated.Succeeded);
        Assert.IsNull(updated.ErrorMessage);
        Assert.IsNotNull(dataService.CreatedBroker);
        Assert.IsNotNull(dataService.UpdatedBroker);
        AssertBrokerMatchesRecord(broker, dataService.CreatedBroker);
        AssertBrokerMatchesRecord(broker, dataService.UpdatedBroker);
    }

    [TestMethod]
    public void DeleteBroker_DelegatesIdAndReturnsSuccess()
    {
        var dataService = new FakeBrokerDataService();
        var service = new BrokerService(dataService);

        var result = service.DeleteBroker(42);

        Assert.IsTrue(result.Succeeded);
        Assert.IsNull(result.ErrorMessage);
        Assert.AreEqual(42, dataService.DeletedBrokerId);
    }

    [TestMethod]
    public void UpdateBroker_RejectsNullBroker()
    {
        var service = new BrokerService(new FakeBrokerDataService());

        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateBroker(null!));
    }

    [TestMethod]
    public void Constructor_RejectsNullDataService()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new BrokerService(null!));
    }

    [TestMethod]
    public void GetAllUpdateAndDelete_PropagateDataServiceExceptions()
    {
        var expected = new InvalidOperationException("Database failure.");
        var service = new BrokerService(new FakeBrokerDataService { Exception = expected });

        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetAllBrokers("all", 2)));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.UpdateBroker(new Broker())));
        Assert.AreSame(expected, Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteBroker(8)));
    }

    private static void AssertBrokerMatchesRecord(Broker broker, BrokerRecord record)
    {
        Assert.AreEqual(record.BrokerId, broker.BrokerId);
        Assert.AreEqual(record.CompanyId, broker.CompanyId);
        Assert.AreEqual(record.BrokerCode, broker.BrokerCode);
        Assert.AreEqual(record.BrokerName, broker.BrokerName);
        Assert.AreEqual(record.Address, broker.Address);
        Assert.AreEqual(record.Email, broker.Email);
        Assert.AreEqual(record.Tel, broker.Tel);
        Assert.AreEqual(record.ContactPerson, broker.ContactPerson);
        Assert.AreEqual(record.ModifiedDate, broker.ModifiedDate);
        CollectionAssert.AreEqual(record.RowVersion, broker.RowVersion);
        Assert.AreEqual(record.User, broker.User);
    }

    private sealed class FakeBrokerDataService : IBrokerDataService
    {
        public BrokerRecord? Broker { get; init; }
        public IReadOnlyList<BrokerRecord> Brokers { get; init; } = [];
        public bool CreateResult { get; init; } = true;
        public Exception? Exception { get; init; }
        public int? RequestedBrokerId { get; private set; }
        public int? DeletedBrokerId { get; private set; }
        public string? SearchText { get; private set; }
        public int? SearchCompanyId { get; private set; }
        public BrokerRecord? CreatedBroker { get; private set; }
        public BrokerRecord? UpdatedBroker { get; private set; }

        public BrokerRecord? GetBrokerById(int brokerId)
        {
            ThrowIfConfigured();
            RequestedBrokerId = brokerId;
            return Broker;
        }

        public IReadOnlyList<BrokerRecord> GetAllBrokers(string searchText, int companyId)
        {
            ThrowIfConfigured();
            SearchText = searchText;
            SearchCompanyId = companyId;
            return Brokers;
        }

        public bool CreateBroker(BrokerRecord broker)
        {
            ThrowIfConfigured();
            CreatedBroker = broker;
            return CreateResult;
        }

        public bool UpdateBroker(BrokerRecord broker)
        {
            ThrowIfConfigured();
            UpdatedBroker = broker;
            return CreateResult;
        }

        public bool DeleteBroker(int brokerId)
        {
            ThrowIfConfigured();
            DeletedBrokerId = brokerId;
            return CreateResult;
        }

        private void ThrowIfConfigured()
        {
            if (Exception is not null)
            {
                throw Exception;
            }
        }
    }
}
