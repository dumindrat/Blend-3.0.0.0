using Blend.BusinessService.Purchases;
using Blend.DataService.Documents;
using Blend.DataService.Purchases;

namespace Blend.BusinessService.Tests.Purchases;

[TestClass]
public sealed class PurchaseServiceTests
{
    [TestMethod]
    public void AddSingleLineCalculatesValuesAndFormatsLockedNumber()
    {
        var data = new FakeData();
        var service = new PurchaseService(data);
        Assert.IsTrue(service.AddStraightLine(ValidLine()).Succeeded);
        Assert.AreEqual(2, data.CompanyId);
        Assert.AreEqual(98m, data.First!.Weight);
        Assert.AreEqual(2m, data.First.ReducedQty);
        Assert.AreEqual(196m, data.First.Value);
        Assert.AreEqual(98m, data.First.VirtualWeight);
        Assert.AreEqual(196m, data.First.VirtualValue);
        Assert.IsNull(data.Repeated);
        Assert.AreEqual("0042P", data.FormattedNumber);
    }

    [TestMethod]
    public void RepeatedLineCalculationsAndVersionsAreMappedForUpdate()
    {
        var data = new FakeData();
        var line = ValidLine();
        line.Id = 10; line.StockId = 20; line.RowVersion1 = [1];
        line.RepeatingId = 11; line.RepeatingStockId = 21; line.RowVersion2 = [2];
        line.Net2 = 4; line.Pack2 = 5;
        Assert.IsTrue(new PurchaseService(data).UpdateStraightLine(line).Succeeded);
        Assert.AreEqual(100m, data.First!.Weight);
        Assert.AreEqual(0m, data.First.ReducedQty);
        Assert.AreEqual(118m, data.First.VirtualWeight);
        Assert.AreEqual(236m, data.First.VirtualValue);
        Assert.AreEqual((byte)1, data.First.RowVersion![0]);
        Assert.AreEqual(11L, data.Repeated!.Id);
        Assert.AreEqual(21L, data.Repeated.StockId);
        Assert.AreEqual((byte)2, data.Repeated.RowVersion![0]);
        Assert.AreEqual(18m, data.Repeated.Weight);
        Assert.AreEqual(36m, data.Repeated.Value);
        Assert.AreEqual(2m, data.Repeated.ReducedQty);
        Assert.AreEqual(0m, data.Repeated.VirtualWeight);
    }

    [TestMethod]
    public void RejectsInvalidQuantitiesAndPropagatesFailures()
    {
        var data = new FakeData();
        var service = new PurchaseService(data);
        var line = ValidLine();
        line.Net2 = 1;
        Assert.ThrowsExactly<ArgumentException>(() => service.AddStraightLine(line));
        line.Net2 = 0; line.ReducedQty = 101;
        Assert.ThrowsExactly<ArgumentException>(() => service.AddStraightLine(line));
        line.ReducedQty = 2;
        var failure = new InvalidOperationException("database error");
        data.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.AddStraightLine(line)));
        data.Failure = null; data.Success = false;
        Assert.IsFalse(service.UpdateStraightLine(line).Succeeded);
    }

    [TestMethod]
    public void SearchAndSaleWritesMapValues()
    {
        var data = new FakeData { Sales = [new PurchaseSaleRecord { Id = 9, SaleDate = "2026-01-01", IsClosed = true, SaleStatus = "Closed" }] };
        var service = new PurchaseService(data);
        var sales = service.GetAllSales("10~2026", 2);
        Assert.AreEqual("10/2026", data.Search);
        Assert.AreEqual(2, sales[0].CompanyId);
        Assert.AreEqual(9, sales[0].SaleId);
        service.GetAllSales("all", 1);
        Assert.AreEqual(string.Empty, data.Search);
        var sale = new PurchaseSale { SaleId = 9, CompanyId = 2, SaleDate = "2026-01-01", User = "operator" };
        Assert.IsTrue(service.CreateSale(sale).Succeeded);
        Assert.AreEqual(2, data.Sale!.CompanyId);
        Assert.IsTrue(service.UpdateSale(sale).Succeeded);
        Assert.AreEqual(9, data.Sale!.Id);
        Assert.IsTrue(service.CloseSale(sale).Succeeded);
        Assert.AreEqual(true, data.Closed);
        Assert.IsTrue(service.OpenSale(sale).Succeeded);
        Assert.AreEqual(false, data.Closed);
        data.Success = false;
        Assert.IsFalse(service.CreateSale(sale).Succeeded);
        Assert.IsFalse(service.CloseSale(sale).Succeeded);
    }

    [TestMethod]
    public void ReservedNumberExcludesVoidRowsAndMapsRepeatedLine()
    {
        var first = new PurchaseLineRecord { Id = 10, SaleId = 1, NetWeight = 10, Packs = 5, VirtualWeight = 68, VirtualValue = 136, RowVersion = [1] };
        var repeated = new PurchaseLineRecord { Id = 11, SaleId = 1, IsRepeated = true, StockId = 21, NetWeight = 4, Packs = 5, ReducedQty = 2, RowVersion = [2] };
        var voidLine = new PurchaseLineRecord { Id = 12, SaleId = 1, IsRepeated = true };
        var data = new FakeData { Reserved = [first, repeated, voidLine], Lines = [first, repeated] };
        var service = new PurchaseService(data);
        Assert.IsNull(service.GetStraightLineByReservedNo(12));
        Assert.IsNull(service.GetStraightLineByReservedNo(13));
        var line = service.GetStraightLineByReservedNo(11)!;
        Assert.AreEqual(10L, line.Id);
        Assert.AreEqual(10m, line.Net1);
        Assert.AreEqual(68m, line.Weight);
        Assert.AreEqual(11L, line.RepeatingId);
        Assert.AreEqual(21L, line.RepeatingStockId);
        Assert.AreEqual(2m, line.ReducedQty);
        Assert.AreEqual((byte)2, line.RowVersion2![0]);
        data.Lines = [first, repeated, voidLine];
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetStraightLineByReservedNo(11));
    }

    [TestMethod]
    public void DisplayHidesRepeatedValuesAndDeleteReloadsIdentity()
    {
        var raw = new PurchaseLineRecord { Id = 11, SaleId = 1, StockId = 21, BoxNo = "BOX", Grade = "A", Rate = 2, ReducedQty = 3, IsRepeated = true };
        var data = new FakeData { Lines = [raw], Reserved = [raw] };
        var service = new PurchaseService(data);
        var line = service.GetAddedStraightLines(1)[0];
        Assert.AreEqual(string.Empty, line.BoxNo);
        Assert.IsNull(line.SaleId);
        Assert.IsNull(line.Rate);
        Assert.AreEqual(3m, line.ReducedQty);
        line.User = "operator";
        Assert.IsTrue(service.DeleteStraightLine(line).Succeeded);
        Assert.AreEqual("BOX", data.Deleted!.BoxNo);
        Assert.AreEqual(1, data.Deleted.SaleId);
        Assert.AreEqual(21L, data.Deleted.StockId);
        Assert.AreEqual("operator", data.Deleted.User);
        data.Success = false;
        Assert.IsFalse(service.DeleteStraightLine(line).Succeeded);
    }

    [TestMethod]
    public void CertificatesAndNotFoundAreExplicit()
    {
        var data = new FakeData { Certificates = [new TeaCertificateRecord { CertifiedName = "Organic" }] };
        var service = new PurchaseService(data);
        Assert.AreEqual("Organic", service.GetTeaCertificates()[0].CertifiedName);
        Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteStraightLine(new StraightLine { Id = 42, User = "operator" }));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => service.GetAllSales("all", 0));
    }

    private static StraightLine ValidLine() => new() { CompanyId = 2, SaleId = 1, BrokerId = 3, User = "operator", Grade = "A", GardenMark = "M", Net1 = 10, Pack1 = 10, ReducedQty = 2, Rate = 2 };

    private sealed class FakeData : IPurchaseDataService
    {
        public string? Search { get; set; }
        public int CompanyId { get; set; }
        public string? FormattedNumber { get; set; }
        public PurchaseLineRecord? First { get; set; }
        public PurchaseLineRecord? Repeated { get; set; }
        public PurchaseLineRecord? Deleted { get; set; }
        public PurchaseSaleRecord? Sale { get; set; }
        public bool? Closed { get; set; }
        public bool Success { get; set; } = true;
        public Exception? Failure { get; set; }
        public IReadOnlyList<PurchaseSaleRecord> Sales { get; set; } = [];
        public IReadOnlyList<PurchaseLineRecord> Lines { get; set; } = [];
        public IReadOnlyList<PurchaseLineRecord> Reserved { get; set; } = [];
        public IReadOnlyList<TeaCertificateRecord> Certificates { get; set; } = [];
        public IReadOnlyList<PurchaseSaleRecord> GetSales(string search, int companyId) { Search = search; CompanyId = companyId; return Sales; }
        public IReadOnlyList<PurchaseLineRecord> GetLinesBySale(int saleId) => Lines;
        public IReadOnlyList<PurchaseLineRecord> GetLinesByReservedNo(long id) => Reserved;
        public IReadOnlyList<TeaCertificateRecord> GetTeaCertificates() => Certificates;
        public bool CreateSale(PurchaseSaleRecord sale) { Sale = sale; return Success; }
        public bool UpdateSale(PurchaseSaleRecord sale) { Sale = sale; return Success; }
        public bool SetSaleClosed(PurchaseSaleRecord sale, bool closed) { Sale = sale; Closed = closed; return Success; }
        public string AddLines(int companyId, PurchaseLineRecord first, PurchaseLineRecord? repeated, Func<DocumentRecord, string> formatDocumentNumber)
        {
            if (Failure is not null) throw Failure;
            CompanyId = companyId; First = first; Repeated = repeated;
            return FormattedNumber = formatDocumentNumber(new DocumentRecord { DocNo = 42, DocFormat = "0000", Prefix = "P" });
        }
        public bool UpdateLines(PurchaseLineRecord first, PurchaseLineRecord? repeated) { First = first; Repeated = repeated; return Success; }
        public bool DeleteLine(PurchaseLineRecord line) { Deleted = line; return Success; }
    }
}
