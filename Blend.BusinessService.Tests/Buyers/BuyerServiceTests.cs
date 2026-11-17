using Blend.BusinessService.Buyers;
using Blend.DataService.Buyers;

namespace Blend.BusinessService.Tests.Buyers;

[TestClass]
public sealed class BuyerServiceTests
{
    [TestMethod]
    public void GetAllBuyers_MapsRecordsAndForwardsArguments()
    {
        var data = new FakeBuyerDataService();
        data.Records.Add(new BuyerRecord
        {
            BuyerId = 5, CompanyId = 2, BuyerCode = "B1", BuyerName = "Name", Address = "Addr",
            Email = "b@example.com", ModifiedDate = "today", RowVersion = [1], User = "op"
        });

        var buyer = new BuyerService(data).GetAllBuyers("find", 2).Single();

        Assert.AreEqual(("find", 2), data.LastSearch);
        Assert.AreEqual(5, buyer.BuyerId);
        Assert.AreEqual(2, buyer.CompanyId);
        Assert.AreEqual("B1", buyer.BuyerCode);
        Assert.AreEqual("Name", buyer.BuyerName);
        Assert.AreEqual("Addr", buyer.Address);
        Assert.AreEqual("b@example.com", buyer.Email);
        Assert.AreEqual("today", buyer.ModifiedDate);
        CollectionAssert.AreEqual(new byte[] { 1 }, buyer.RowVersion);
        Assert.AreEqual("op", buyer.User);
    }

    [TestMethod]
    public void CreateAndUpdate_MapBuyerToRecord()
    {
        var data = new FakeBuyerDataService();
        var service = new BuyerService(data);
        var buyer = new Buyer { BuyerId = 6, CompanyId = 3, BuyerCode = "B2", BuyerName = "Name", RowVersion = [9], User = "op" };

        Assert.IsTrue(service.CreateBuyer(buyer).Succeeded);
        Assert.AreEqual(6, data.Created!.BuyerId);
        Assert.AreEqual("B2", data.Created.BuyerCode);

        Assert.IsTrue(service.UpdateBuyer(buyer).Succeeded);
        Assert.AreEqual(3, data.Updated!.CompanyId);
        CollectionAssert.AreEqual(new byte[] { 9 }, data.Updated.RowVersion);
    }

    [TestMethod]
    public void DataServiceReturningFalse_ProducesFailureResults()
    {
        var service = new BuyerService(new FakeBuyerDataService { Result = false });

        Assert.AreEqual("The buyer could not be created.", service.CreateBuyer(new Buyer()).ErrorMessage);
        Assert.AreEqual("The buyer could not be updated.", service.UpdateBuyer(new Buyer()).ErrorMessage);
        Assert.AreEqual("The buyer could not be deleted.", service.DeleteBuyer(1).ErrorMessage);
    }

    [TestMethod]
    public void DeleteBuyer_ForwardsId()
    {
        var data = new FakeBuyerDataService();

        Assert.IsTrue(new BuyerService(data).DeleteBuyer(11).Succeeded);
        Assert.AreEqual(11, data.DeletedId);
    }

    [TestMethod]
    public void NullArgumentsAreRejected()
    {
        var service = new BuyerService(new FakeBuyerDataService());

        Assert.ThrowsExactly<ArgumentNullException>(() => new BuyerService(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateBuyer(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateBuyer(null!));
    }

    private sealed class FakeBuyerDataService : IBuyerDataService
    {
        public List<BuyerRecord> Records { get; } = [];
        public (string, int)? LastSearch { get; private set; }
        public BuyerRecord? Created { get; private set; }
        public BuyerRecord? Updated { get; private set; }
        public int? DeletedId { get; private set; }
        public bool Result { get; init; } = true;

        public IReadOnlyList<BuyerRecord> GetAllBuyers(string searchText, int companyId)
        {
            LastSearch = (searchText, companyId);
            return Records;
        }

        public bool CreateBuyer(BuyerRecord buyer) { Created = buyer; return Result; }
        public bool UpdateBuyer(BuyerRecord buyer) { Updated = buyer; return Result; }
        public bool DeleteBuyer(int buyerId) { DeletedId = buyerId; return Result; }
    }
}
