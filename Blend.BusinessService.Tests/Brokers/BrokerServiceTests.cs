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
            RowVersion = [1, 2, 3]
        };
        var service = new BrokerService(new FakeBrokerDataService { Broker = record });

        var result = service.GetBrokerById(42);

        Assert.IsNotNull(result);
        Assert.AreEqual(record.BrokerId, result.BrokerId);
        Assert.AreEqual(record.CompanyId, result.CompanyId);
        Assert.AreEqual(record.BrokerCode, result.BrokerCode);
        Assert.AreEqual(record.BrokerName, result.BrokerName);
        CollectionAssert.AreEqual(record.RowVersion, result.RowVersion);
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

    private sealed class FakeBrokerDataService : IBrokerDataService
    {
        public BrokerRecord? Broker { get; init; }
        public bool CreateResult { get; init; } = true;
        public Exception? Exception { get; init; }

        public BrokerRecord? GetBrokerById(int brokerId)
        {
            ThrowIfConfigured();
            return Broker;
        }

        public IReadOnlyList<BrokerRecord> GetAllBrokers(string searchText, int companyId)
        {
            ThrowIfConfigured();
            return Array.Empty<BrokerRecord>();
        }

        public bool CreateBroker(BrokerRecord broker)
        {
            ThrowIfConfigured();
            return CreateResult;
        }

        public bool UpdateBroker(BrokerRecord broker)
        {
            ThrowIfConfigured();
            return CreateResult;
        }

        public bool DeleteBroker(int brokerId)
        {
            ThrowIfConfigured();
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
