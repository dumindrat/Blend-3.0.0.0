using Blend.BusinessService.TeaStandards;
using Blend.DataService.TeaStandards;

namespace Blend.BusinessService.Tests.TeaStandards;

[TestClass]
public sealed class TeaStandardServiceTests
{
    [TestMethod]
    public void GetAllStandards_MapsRecords()
    {
        var rowVersion = new byte[] { 1, 2 };
        var records = new[] { new TeaStandardRecord { StandardId = 7, StandardName = "Premium", RowVersion = rowVersion, User = "operator" } };
        var dataService = new FakeDataService { Standards = records };

        var result = new TeaStandardService(dataService).GetAllStandards("Premium", 2);

        Assert.AreEqual("Premium", dataService.SearchText);
        Assert.AreEqual(2, dataService.CompanyId);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(7, result[0].StandardId);
        Assert.AreEqual("Premium", result[0].StandardName);
        Assert.AreSame(rowVersion, result[0].RowVersion);
        Assert.AreEqual("operator", result[0].User);
    }

    [TestMethod]
    public void CreateAndUpdate_MapStandardAndReturnSuccess()
    {
        var dataService = new FakeDataService();
        var service = new TeaStandardService(dataService);
        var standard = new TeaStandard { StandardId = 12, StandardName = "Premium", RowVersion = [1], User = "operator" };

        Assert.IsTrue(service.CreateStandard(standard, 3).Succeeded);
        Assert.IsTrue(service.UpdateStandard(standard).Succeeded);
        Assert.AreEqual("Premium", dataService.Created?.StandardName);
        Assert.AreEqual("operator", dataService.Created?.User);
        Assert.AreEqual(3, dataService.CreateCompanyId);
        Assert.AreEqual(12, dataService.Updated?.StandardId);
        CollectionAssert.AreEqual(new byte[] { 1 }, dataService.Updated?.RowVersion);
    }

    [TestMethod]
    public void OperationsReturnFailureWhenDataServiceReportsFailure()
    {
        var service = new TeaStandardService(new FakeDataService { Succeeded = false });

        Assert.AreEqual("The standard could not be created.", service.CreateStandard(new TeaStandard(), 1).ErrorMessage);
        Assert.AreEqual("The standard could not be updated.", service.UpdateStandard(new TeaStandard()).ErrorMessage);
        Assert.AreEqual("The standard could not be deleted.", service.DeleteStandard(3).ErrorMessage);
    }

    [TestMethod]
    public void DeleteStandardDelegatesId()
    {
        var dataService = new FakeDataService();

        Assert.IsTrue(new TeaStandardService(dataService).DeleteStandard(16).Succeeded);

        Assert.AreEqual(16, dataService.DeletedId);
    }

    [TestMethod]
    public void ConstructorAndModelArgumentsRejectNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new TeaStandardService(null!));
        var service = new TeaStandardService(new FakeDataService());
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateStandard(null!, 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateStandard(null!));
    }

    private sealed class FakeDataService : ITeaStandardDataService
    {
        public IReadOnlyList<TeaStandardRecord> Standards { get; init; } = [];
        public bool Succeeded { get; init; } = true;
        public string? SearchText { get; private set; }
        public int? CompanyId { get; private set; }
        public TeaStandardRecord? Created { get; private set; }
        public int? CreateCompanyId { get; private set; }
        public TeaStandardRecord? Updated { get; private set; }
        public int? DeletedId { get; private set; }

        public IReadOnlyList<TeaStandardRecord> GetAllStandards(string searchText, int companyId)
        {
            SearchText = searchText;
            CompanyId = companyId;
            return Standards;
        }

        public bool CreateStandard(TeaStandardRecord standard, int companyId)
        {
            Created = standard;
            CreateCompanyId = companyId;
            return Succeeded;
        }

        public bool UpdateStandard(TeaStandardRecord standard)
        {
            Updated = standard;
            return Succeeded;
        }

        public bool DeleteStandard(int standardId)
        {
            DeletedId = standardId;
            return Succeeded;
        }
    }
}
