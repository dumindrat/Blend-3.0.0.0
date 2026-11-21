using Blend.BusinessService.Documents;
using Blend.DataService.Purchases;

namespace Blend.BusinessService.Purchases;

public sealed class PurchaseService : IPurchaseService
{
    private readonly IPurchaseDataService _dataService;

    public PurchaseService(IPurchaseDataService dataService) =>
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public IReadOnlyList<PurchaseSale> GetAllSales(string searchString, int companyId)
    {
        ArgumentNullException.ThrowIfNull(searchString);
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        var search = searchString == "all" ? string.Empty : searchString.Replace('~', '/');
        return _dataService.GetSales(search, companyId).Select(record => new PurchaseSale
        {
            SaleId = record.Id, CompanyId = companyId, SaleDate = record.SaleDate, IsClosed = record.IsClosed,
            SaleStatus = record.SaleStatus, ModifiedDate = record.ModifiedDate, ModifiedUser = record.ModifiedUser
        }).ToArray();
    }

    public IReadOnlyList<StraightLine> GetAddedStraightLines(int saleId)
    {
        if (saleId <= 0) throw new ArgumentOutOfRangeException(nameof(saleId));
        return _dataService.GetLinesBySale(saleId).Select(record =>
        {
            var line = ToLine(record);
            if (line.IsRepeated)
            {
                line.BoxNo = line.Grade = line.Standard = line.SaleCode = line.BrokerCode = line.Invoice = line.LotNo = line.GardenMark = string.Empty;
                line.BrokerId = line.SaleId = null;
                line.SaleType = null;
                line.Rate = line.Weight = line.Value = line.VirtualValue = line.VirtualWeight = null;
            }
            return line;
        }).ToArray();
    }

    public StraightLine? GetStraightLineByReservedNo(long straightLineId)
    {
        if (straightLineId <= 0) throw new ArgumentOutOfRangeException(nameof(straightLineId));
        var records = _dataService.GetLinesByReservedNo(straightLineId);
        var selected = records.SingleOrDefault(record => record.Id == straightLineId);
        if (selected is null) return null;
        if (selected.SaleId is null || selected.SaleId <= 0)
            throw new InvalidOperationException("The purchase line has no valid sale.");
        var activeIds = _dataService.GetLinesBySale(selected.SaleId.Value).Select(record => record.Id).ToHashSet();
        if (!activeIds.Contains(straightLineId)) return null;
        var active = records.Where(record => activeIds.Contains(record.Id)).ToArray();
        if (active.Length == 0 || active.Length > 2 || active[0].IsRepeated || (active.Length == 2 && !active[1].IsRepeated))
            throw new InvalidOperationException("The purchase line group cannot be edited safely.");
        var line = ToLine(active[0]);
        line.Net1 = active[0].NetWeight;
        line.Pack1 = active[0].Packs;
        line.Weight = active[0].VirtualWeight;
        line.Value = active[0].VirtualValue;
        line.RowVersion1 = active[0].RowVersion;
        if (active.Length == 2)
        {
            var repeated = active[1];
            line.RepeatingId = repeated.Id;
            line.RepeatingStockId = repeated.StockId;
            line.Net2 = repeated.NetWeight;
            line.Pack2 = repeated.Packs;
            line.RowVersion2 = repeated.RowVersion;
            line.ReducedQty = repeated.ReducedQty;
        }
        return line;
    }

    public IReadOnlyList<TeaCertificate> GetTeaCertificates() =>
        _dataService.GetTeaCertificates().Select(record => new TeaCertificate { CertifiedName = record.CertifiedName }).ToArray();

    public BusinessOperationResult CreateSale(PurchaseSale sale)
    {
        ValidateSale(sale, requireId: false, requireDate: true);
        return Result(_dataService.CreateSale(ToRecord(sale)), "The sale could not be created.");
    }

    public BusinessOperationResult UpdateSale(PurchaseSale sale)
    {
        ValidateSale(sale, requireId: true, requireDate: true);
        return Result(_dataService.UpdateSale(ToRecord(sale)), "The sale could not be updated.");
    }

    public BusinessOperationResult CloseSale(PurchaseSale sale) => SetClosed(sale, true);
    public BusinessOperationResult OpenSale(PurchaseSale sale) => SetClosed(sale, false);

    public BusinessOperationResult AddStraightLine(StraightLine line)
    {
        var (first, repeated) = CalculateLines(line);
        _dataService.AddLines(line.CompanyId, first, repeated, DocumentService.FormatDocumentNumber);
        return BusinessOperationResult.Success();
    }

    public BusinessOperationResult UpdateStraightLine(StraightLine line)
    {
        var (first, repeated) = CalculateLines(line);
        return Result(_dataService.UpdateLines(first, repeated), "The purchase line could not be updated.");
    }

    public BusinessOperationResult DeleteStraightLine(StraightLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (line.Id <= 0) throw new ArgumentException("A purchase line ID is required.", nameof(line));
        ArgumentException.ThrowIfNullOrWhiteSpace(line.User);
        var records = _dataService.GetLinesByReservedNo(line.Id);
        var record = records.SingleOrDefault(item => item.Id == line.Id)
            ?? throw new InvalidOperationException("The purchase line was not found.");
        record.User = line.User;
        return Result(_dataService.DeleteLine(record), "The purchase line could not be deleted.");
    }

    private BusinessOperationResult SetClosed(PurchaseSale sale, bool closed)
    {
        ValidateSale(sale, requireId: true, requireDate: false);
        return Result(_dataService.SetSaleClosed(ToRecord(sale), closed), closed ? "The sale could not be closed." : "The sale could not be opened.");
    }

    private static void ValidateSale(PurchaseSale sale, bool requireId, bool requireDate)
    {
        ArgumentNullException.ThrowIfNull(sale);
        ArgumentException.ThrowIfNullOrWhiteSpace(sale.User);
        if (requireDate) ArgumentException.ThrowIfNullOrWhiteSpace(sale.SaleDate);
        if ((!requireId && sale.CompanyId <= 0) || (requireId && sale.SaleId <= 0))
            throw new ArgumentException("A valid company or sale ID is required.", nameof(sale));
    }

    private static (PurchaseLineRecord First, PurchaseLineRecord? Repeated) CalculateLines(StraightLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.User);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.Grade);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.GardenMark);
        if (line.SaleId is null or <= 0 || line.BrokerId is null or <= 0 || line.Rate is null || line.ReducedQty is null ||
            line.Net1 <= 0 || line.Pack1 <= 0 || line.Rate < 0 || line.ReducedQty < 0 || line.Net2 < 0 || line.Pack2 < 0 || (line.Net2 > 0) != (line.Pack2 > 0))
            throw new ArgumentException("Valid sale, broker, pack, weight, reduction and rate values are required.", nameof(line));
        var hasRepeated = line.Net2 > 0;
        var firstWeight = line.Net1 * line.Pack1 - (hasRepeated ? 0 : line.ReducedQty.Value);
        var secondWeight = hasRepeated ? line.Net2 * line.Pack2 - line.ReducedQty.Value : 0;
        if (firstWeight < 0 || secondWeight < 0) throw new ArgumentException("Reduction cannot exceed the line weight.", nameof(line));
        var total = firstWeight + secondWeight;
        var first = ToRecord(line);
        first.NetWeight = line.Net1; first.Packs = line.Pack1; first.ReducedQty = hasRepeated ? 0 : line.ReducedQty;
        first.Weight = firstWeight; first.Value = firstWeight * line.Rate.Value; first.VirtualWeight = total; first.VirtualValue = total * line.Rate.Value; first.RowVersion = line.RowVersion1;
        if (!hasRepeated) return (first, null);
        var repeated = ToRecord(line);
        repeated.Id = line.RepeatingId; repeated.StockId = line.RepeatingStockId; repeated.RowVersion = line.RowVersion2;
        repeated.NetWeight = line.Net2; repeated.Packs = line.Pack2; repeated.ReducedQty = line.ReducedQty;
        repeated.Weight = secondWeight; repeated.Value = secondWeight * line.Rate.Value; repeated.VirtualWeight = 0; repeated.VirtualValue = 0; repeated.IsRepeated = true;
        return (first, repeated);
    }

    private static BusinessOperationResult Result(bool success, string message) => success ? BusinessOperationResult.Success() : BusinessOperationResult.Failure(message);

    private static PurchaseSaleRecord ToRecord(PurchaseSale sale) => new()
    {
        Id = sale.SaleId, CompanyId = sale.CompanyId, SaleDate = sale.SaleDate, User = sale.User
    };

    private static StraightLine ToLine(PurchaseLineRecord record) => new()
    {
        Id = record.Id, StockId = record.StockId, BoxNo = record.BoxNo, SaleId = record.SaleId, SaleCode = record.SaleCode,
        Invoice = record.Invoice, LotNo = record.LotNo, BrokerId = record.BrokerId, BrokerCode = record.BrokerCode,
        SaleType = record.SaleType, Grade = record.Grade, Standard = record.Standard, GardenMark = record.GardenMark,
        TeaCertificates = record.TeaCertificates, NetWeight = record.NetWeight, Packs = record.Packs, Weight = record.Weight,
        ReducedQty = record.ReducedQty, Rate = record.Rate, Value = record.Value, TinNo = record.TinNo,
        IsRepeated = record.IsRepeated, VirtualWeight = record.VirtualWeight, VirtualValue = record.VirtualValue,
        RowVersion = record.RowVersion, User = record.User
    };

    private static PurchaseLineRecord ToRecord(StraightLine line) => new()
    {
        Id = line.Id, StockId = line.StockId, BoxNo = line.BoxNo, SaleId = line.SaleId, SaleCode = line.SaleCode,
        Invoice = line.Invoice, LotNo = line.LotNo, BrokerId = line.BrokerId, BrokerCode = line.BrokerCode,
        SaleType = line.SaleType, Grade = line.Grade, Standard = line.Standard, GardenMark = line.GardenMark,
        TeaCertificates = line.TeaCertificates, Rate = line.Rate, User = line.User
    };
}
