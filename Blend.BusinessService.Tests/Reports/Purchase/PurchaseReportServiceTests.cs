using Blend.BusinessService.Reports.Purchase;
using Blend.DataService.Reports.Purchase;

namespace Blend.BusinessService.Tests.Reports.Purchase;

[TestClass]
public sealed class PurchaseReportServiceTests
{
    [TestMethod]
    public void SaleReportsPassSaleIdAndMapEveryFieldInOrder()
    {
        var summaries = new[] { Filled<SaleSummaryRecord>(1), Filled<SaleSummaryRecord>(2) };
        var sheets = new[] { Filled<PurchaseSheetRecord>(1), Filled<PurchaseSheetRecord>(2) };
        var canisters = new[] { Filled<CanisterSheetRecord>(1), Filled<CanisterSheetRecord>(2) };
        var data = new FakeData { Summaries = summaries, Sheets = sheets, Canisters = canisters };
        var service = new PurchaseReportService(data);

        var summary = service.GetSaleSummary(11);
        Assert.AreEqual(("summary", 11), (data.Method, data.SaleId));
        CheckRecords(summaries, summary);
        var sheet = service.GetPurchaseSheet(12);
        Assert.AreEqual(("sheet", 12), (data.Method, data.SaleId));
        CheckRecords(sheets, sheet);
        var canister = service.GetCanisterSheet(13);
        Assert.AreEqual(("canister", 13), (data.Method, data.SaleId));
        CheckRecords(canisters, canister);
    }

    [TestMethod]
    public void FiltersArePassedUnchangedAndSheetsAreMapped()
    {
        var sheets = new[] { Filled<PurchaseSheetRecord>(1), Filled<PurchaseSheetRecord>(2) };
        var data = new FakeData { Sheets = sheets };
        var service = new PurchaseReportService(data);
        var filter = new PurchaseSheetFilter { FromSaleDate = "2026-01-01", ToSaleDate = "2026-02-01", Grade = "G", Standard = "S", SaleType = "MS", BrokerCode = "B" };
        var advanced = service.GetPurchaseSheetByAdvanceFilter(filter);
        Assert.AreEqual("advanced", data.Method);
        CollectionAssert.AreEqual(new object?[] { "2026-01-01", "2026-02-01", "G", "S", "MS", "B" }, data.Arguments);
        CheckRecords(sheets, advanced);

        var broker = service.GetPurchaseSheetByBroker(new BrokerPurchaseFilter { FromSaleDate = "2026-03-01", ToSaleDate = "2026-04-01", BrokerId = 7 });
        Assert.AreEqual("broker", data.Method);
        CollectionAssert.AreEqual(new object?[] { "2026-03-01", "2026-04-01", 7 }, data.Arguments);
        CheckRecords(sheets, broker);

        service.GetPurchaseSheetByAdvanceFilter(new PurchaseSheetFilter());
        CollectionAssert.AreEqual(new object?[] { null, null, null, null, null, null }, data.Arguments);
        service.GetPurchaseSheetByBroker(new BrokerPurchaseFilter());
        CollectionAssert.AreEqual(new object?[] { null, null, 0 }, data.Arguments);
    }

    [TestMethod]
    public void EmptyResultsRemainEmptyAndNullValuesArePreserved()
    {
        var data = new FakeData();
        var service = new PurchaseReportService(data);
        Assert.AreEqual(0, service.GetSaleSummary(1).Count);
        Assert.AreEqual(0, service.GetPurchaseSheet(1).Count);
        Assert.AreEqual(0, service.GetCanisterSheet(1).Count);
        Assert.AreEqual(0, service.GetPurchaseSheetByAdvanceFilter(new PurchaseSheetFilter()).Count);
        Assert.AreEqual(0, service.GetPurchaseSheetByBroker(new BrokerPurchaseFilter()).Count);
        data.Sheets = [new PurchaseSheetRecord { BoxNo = null, TeaCertificates = null }];
        Assert.IsNull(service.GetPurchaseSheet(1)[0].BoxNo);
        Assert.IsNull(service.GetPurchaseSheet(1)[0].TeaCertificates);
    }

    [TestMethod]
    public void InvalidInputsAreRejectedAndDatabaseErrorsPropagate()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PurchaseReportService(null!));
        var data = new FakeData();
        var service = new PurchaseReportService(data);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.GetSaleSummary(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.GetPurchaseSheet(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.GetCanisterSheet(0));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.GetPurchaseSheetByAdvanceFilter(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.GetPurchaseSheetByBroker(null!));
        Assert.IsNull(data.Method);
        var error = new InvalidOperationException("Database failure");
        data.Failure = error;
        Assert.AreSame(error, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetSaleSummary(1)));
        Assert.AreSame(error, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheet(1)));
        Assert.AreSame(error, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetCanisterSheet(1)));
        Assert.AreSame(error, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheetByAdvanceFilter(new PurchaseSheetFilter())));
        Assert.AreSame(error, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheetByBroker(new BrokerPurchaseFilter())));
    }

    private static T Filled<T>(int seed) where T : new()
    {
        var record = new T();
        foreach (var (property, index) in typeof(T).GetProperties().Select((property, index) => (property, index)))
        {
            object value = property.PropertyType == typeof(string) ? $"value-{seed}-{index}" :
                property.PropertyType == typeof(decimal) ? seed * 100m + index + 0.5m :
                property.PropertyType == typeof(int) ? seed * 100 + index :
                property.PropertyType == typeof(bool) ? seed % 2 == 1 : throw new InvalidOperationException("Unexpected record type.");
            property.SetValue(record, value);
        }
        return record;
    }

    private static void CheckRecords<TRecord, TModel>(IReadOnlyList<TRecord> records, IReadOnlyList<TModel> models)
    {
        var recordProperties = typeof(TRecord).GetProperties();
        var modelProperties = typeof(TModel).GetProperties();
        Assert.AreEqual(recordProperties.Length, modelProperties.Length);
        Assert.AreEqual(records.Count, models.Count);
        for (var index = 0; index < records.Count; index++)
        {
            foreach (var property in recordProperties)
            {
                var target = modelProperties.Single(item => item.Name == property.Name);
                Assert.AreEqual(property.GetValue(records[index]), target.GetValue(models[index]), property.Name);
            }
        }
    }

    private sealed class FakeData : IPurchaseReportDataService
    {
        public string? Method { get; private set; }
        public int SaleId { get; private set; }
        public object?[] Arguments { get; private set; } = [];
        public Exception? Failure { get; set; }
        public IReadOnlyList<SaleSummaryRecord> Summaries { get; set; } = [];
        public IReadOnlyList<PurchaseSheetRecord> Sheets { get; set; } = [];
        public IReadOnlyList<CanisterSheetRecord> Canisters { get; set; } = [];

        public IReadOnlyList<SaleSummaryRecord> GetSaleSummary(int saleId) { Record("summary", saleId); return Summaries; }
        public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheet(int saleId) { Record("sheet", saleId); return Sheets; }
        public IReadOnlyList<CanisterSheetRecord> GetCanisterSheet(int saleId) { Record("canister", saleId); return Canisters; }
        public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByAdvanceFilter(string? fromSaleDate, string? toSaleDate, string? grade, string? standard, string? saleType, string? brokerCode)
        {
            Record("advanced", 0, fromSaleDate, toSaleDate, grade, standard, saleType, brokerCode);
            return Sheets;
        }
        public IReadOnlyList<PurchaseSheetRecord> GetPurchaseSheetByBroker(string? fromSaleDate, string? toSaleDate, int brokerId)
        {
            Record("broker", 0, fromSaleDate, toSaleDate, brokerId);
            return Sheets;
        }
        private void Record(string method, int saleId, params object?[] arguments)
        {
            if (Failure is not null) throw Failure;
            Method = method;
            SaleId = saleId;
            Arguments = arguments;
        }
    }
}
