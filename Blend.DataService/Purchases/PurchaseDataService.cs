using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Blend.DataService.Documents;
using Blend.DataService.Mapping;
using Dapper;

namespace Blend.DataService.Purchases;

public sealed class PurchaseDataService : IPurchaseDataService
{
    private readonly IStoredProcedureMapper _mapper;
    private readonly Func<IDbConnection> _connectionFactory;

    public PurchaseDataService(IStoredProcedureMapper mapper, Func<IDbConnection> connectionFactory)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyList<PurchaseSaleRecord> GetSales(string search, int companyId) =>
        _mapper.Query<PurchaseSaleRecord>("trn_PurchaseHeaderGet", new { serachString = search, CompanyId = companyId });

    public IReadOnlyList<PurchaseLineRecord> GetLinesBySale(int saleId) =>
        _mapper.Query<PurchaseLineRecord>("trn_PurchaseDetailGetBySaleId", new { Sale = saleId });

    public IReadOnlyList<PurchaseLineRecord> GetLinesByReservedNo(long id) =>
        _mapper.Query<PurchaseLineRecord>("trn_PurchaseDetailGetByReservedNoId", new { ReservedNoId = id });

    public IReadOnlyList<TeaCertificateRecord> GetTeaCertificates() =>
        _mapper.Query<TeaCertificateRecord>("trn_TeaCertificateGet");

    public bool CreateSale(PurchaseSaleRecord sale)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));
        _mapper.Execute("trn_PurchaseHeaderInsert", new { SaleDates = sale.SaleDate, sale.User, sale.CompanyId });
        return true;
    }

    public bool UpdateSale(PurchaseSaleRecord sale)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));
        _mapper.Execute("trn_PurchaseHeaderUpdate", new { SaleId = sale.Id, sale.SaleDate, sale.User });
        return true;
    }

    public bool SetSaleClosed(PurchaseSaleRecord sale, bool closed)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));
        _mapper.Execute("trn_PurchaseHeaderUpdateByClose", new { SaleId = sale.Id, Status = closed, sale.User });
        return true;
    }

    public string AddLines(int companyId, PurchaseLineRecord first, PurchaseLineRecord? repeated, Func<DocumentRecord, string> formatDocumentNumber)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (formatDocumentNumber is null) throw new ArgumentNullException(nameof(formatDocumentNumber));
        if (companyId <= 0 || !first.SaleId.HasValue || first.SaleId.Value <= 0)
            throw new ArgumentException("A company and sale are required.");
        if (repeated != null && repeated.SaleId != first.SaleId)
            throw new ArgumentException("Both lines must belong to the same sale.", nameof(repeated));

        return InTransaction((connection, transaction) =>
        {
            var saleId = connection.QuerySingleOrDefault<int?>("SELECT Id FROM purchase_header WITH (UPDLOCK, HOLDLOCK) WHERE Id = @SaleId AND CompanyId = @CompanyId", new { first.SaleId, CompanyId = companyId }, transaction);
            if (!saleId.HasValue) throw new InvalidOperationException("The sale was not found in the selected company.");
            var documents = connection.Query<DocumentRecord>("SELECT DocNo, DocFormat, Prefix, DocType, CompanyId FROM documentno WITH (UPDLOCK, HOLDLOCK) WHERE DocType = 1 AND CompanyId = @CompanyId", new { CompanyId = companyId }, transaction).ToArray();
            if (documents.Length != 1) throw new InvalidOperationException("Exactly one purchase document number is required for the company.");
            var boxNo = formatDocumentNumber(documents[0]);
            if (string.IsNullOrWhiteSpace(boxNo) || boxNo.Length > 10)
                throw new InvalidOperationException("The formatted purchase document number must contain at most 10 characters.");
            var tins = connection.Query<int>("SELECT TinNo FROM Tin_No WITH (UPDLOCK, HOLDLOCK)", transaction: transaction).ToArray();
            if (tins.Length != 1) throw new InvalidOperationException("Exactly one tin number is required.");
            Execute(connection, transaction, "trn_TinNoUpdate");
            InsertLine(connection, transaction, first, boxNo, false);
            if (repeated != null) InsertLine(connection, transaction, repeated, boxNo, true);
            Execute(connection, transaction, "trn_DocumentNoGenerate", new { doctypes = 1, CompanyId = companyId });
            Execute(connection, transaction, "trn_GardenMarkInsert", new { GardenMark = first.GardenMark });
            Execute(connection, transaction, "trn_GradeInsert", new { GradeName = first.Grade });
            return boxNo;
        });
    }

    public bool UpdateLines(PurchaseLineRecord first, PurchaseLineRecord? repeated)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (first.SaleId is null || first.SaleId <= 0 || first.Id <= 0 || first.StockId <= 0 || first.RowVersion is null || first.RowVersion.Length != 8)
            throw new ArgumentException("The first line requires a sale, valid line and stock IDs, and an 8-byte row version.", nameof(first));
        if (repeated != null && (repeated.Id <= 0 || repeated.StockId <= 0 || repeated.RowVersion is null || repeated.RowVersion.Length != 8 || repeated.SaleId != first.SaleId || repeated.Id == first.Id || repeated.StockId == first.StockId))
            throw new ArgumentException("The repeated line requires distinct IDs, the same sale, and an 8-byte row version.", nameof(repeated));
        return InTransaction((connection, transaction) =>
        {
            var sale = connection.QuerySingleOrDefault<int?>("SELECT Id FROM purchase_header WITH (UPDLOCK, HOLDLOCK) WHERE Id = @SaleId", new { first.SaleId }, transaction);
            if (!sale.HasValue) throw new InvalidOperationException("The sale was not found.");
            var rows = connection.Query<LineIdentity>("SELECT p.Id, p.BoxNo, p.IsRepeated, s.Id AS StockId FROM purchase_detail p WITH (UPDLOCK, HOLDLOCK) JOIN teastock s WITH (UPDLOCK, HOLDLOCK) ON s.StockItemId = p.Id AND s.ItemType = 'L' WHERE p.SaleId = @SaleId AND p.IsVoid = 0 AND p.BoxNo = (SELECT BoxNo FROM purchase_detail WHERE Id = @Id AND SaleId = @SaleId AND IsVoid = 0) ORDER BY p.Id", new { first.Id, first.SaleId }, transaction).ToArray();
            if (rows.Length != (repeated == null ? 1 : 2) || rows[0].Id != first.Id || rows[0].StockId != first.StockId || rows[0].IsRepeated || (repeated != null && (rows[1].Id != repeated.Id || rows[1].StockId != repeated.StockId || !rows[1].IsRepeated)))
                throw new InvalidOperationException("The purchase lines have changed or do not form the expected group; reload before updating.");
            UpdateLine(connection, transaction, first);
            if (repeated != null) UpdateLine(connection, transaction, repeated);
            return true;
        });
    }

    public bool DeleteLine(PurchaseLineRecord line)
    {
        if (line is null) throw new ArgumentNullException(nameof(line));
        if (line.Id <= 0 || line.StockId <= 0 || line.SaleId is null || line.SaleId <= 0 || string.IsNullOrEmpty(line.BoxNo))
            throw new ArgumentException("Sale, line, stock, and box identifiers are required.", nameof(line));
        return InTransaction((connection, transaction) =>
        {
            var sale = connection.QuerySingleOrDefault<int?>("SELECT Id FROM purchase_header WITH (UPDLOCK, HOLDLOCK) WHERE Id = @SaleId", new { line.SaleId }, transaction);
            if (!sale.HasValue) throw new InvalidOperationException("The sale was not found.");
            var identity = connection.QuerySingleOrDefault<LineIdentity>("SELECT p.Id, p.BoxNo, p.IsRepeated, s.Id AS StockId FROM purchase_detail p WITH (UPDLOCK, HOLDLOCK) JOIN teastock s WITH (UPDLOCK, HOLDLOCK) ON s.StockItemId = p.Id AND s.ItemType = 'L' WHERE p.Id = @Id AND p.SaleId = @SaleId AND p.IsVoid = 0", new { line.Id, line.SaleId }, transaction);
            if (identity == null || identity.StockId != line.StockId || identity.BoxNo != line.BoxNo)
                throw new InvalidOperationException("The purchase line was not found or has changed.");
            var checks = connection.QuerySingle<DeleteChecks>("SELECT dbo.func_IsSaleClose(@SaleId) AS IsClosed, dbo.func_CanEditLine(@StockId) AS CanEdit", new { line.SaleId, line.StockId }, transaction);
            if (checks.IsClosed != false) throw new InvalidOperationException("SALE HAS BEEN CLOSED.UNABLE TO PROCEED");
            if (checks.CanEdit != true) throw new InvalidOperationException("STRAIGHTLINE ALLOCATED TO ANOTHER TRANSACTION");
            connection.Execute("UPDATE purchase_detail SET IsVoid = 1, VirtualWeight = Weight, VirtualValue = Value WHERE Id = @Id AND SaleId = @SaleId; IF @IsRepeated = 0 UPDATE purchase_detail SET IsRepeated = 0 WHERE SaleId = @SaleId AND BoxNo = @BoxNo AND IsRepeated = 1; INSERT INTO purchase_void (PurchaseId, VoidUser, Reason, CreateDate) VALUES (@Id, @User, 'Removed line', GETDATE()); UPDATE teastock SET Qty = 0, Packs = 0 WHERE Id = @StockId", new { line.Id, line.SaleId, line.BoxNo, line.StockId, line.User, identity.IsRepeated }, transaction);
            return true;
        });
    }

    private static void InsertLine(IDbConnection connection, IDbTransaction transaction, PurchaseLineRecord line, string boxNo, bool repeated)
    {
        var parameters = DetailParameters(line);
        parameters.Add("Standards", line.Standard ?? string.Empty);
        parameters.Add("BoxNos", boxNo);
        parameters.Add("Repeated", repeated);
        parameters.Add("lastId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        Execute(connection, transaction, "trn_PurchaseDetailInsert", parameters);
        var id = parameters.Get<int?>("lastId");
        if (!id.HasValue || id.Value <= 0) throw new InvalidOperationException("The purchase insert did not return a line ID.");
        Execute(connection, transaction, "trn_TeaStockInsert", new { stItemId = id.Value, stkItemName = boxNo, stkName = boxNo, stkItemType = "L", stkPack = line.Packs, stkQty = line.Weight });
    }

    private static void UpdateLine(IDbConnection connection, IDbTransaction transaction, PurchaseLineRecord line)
    {
        var parameters = DetailParameters(line);
        parameters.Add("pId", line.Id);
        parameters.Add("stockid", line.StockId);
        parameters.Add("RowVersion", line.RowVersion);
        Execute(connection, transaction, "trn_PrurchaseDetailUpdate", parameters);
        Execute(connection, transaction, "trn_TeaStockUpdateByEditLine", new { stkId = line.StockId, stkPacks = line.Packs, stkQty = line.Weight });
    }

    private static DynamicParameters DetailParameters(PurchaseLineRecord line) => new DynamicParameters(new
    {
        SaleIds = line.SaleId, SaleCodes = line.SaleCode, Invoices = line.Invoice, LotNos = line.LotNo,
        BrokerIds = line.BrokerId, SaleTypes = line.SaleType, Grades = line.Grade, Standards = line.Standard,
        GardenMarks = line.GardenMark, Net = line.NetWeight, Pakets = line.Packs, Weights = line.Weight,
        line.ReducedQty, Rates = line.Rate, Price = line.Value, line.User, VirtualWeights = line.VirtualWeight,
        VirtualValues = line.VirtualValue, Certificates = line.TeaCertificates
    });

    private static void Execute(IDbConnection connection, IDbTransaction transaction, string procedure, object? parameters = null) =>
        connection.Execute(procedure, parameters, transaction, commandType: CommandType.StoredProcedure);

    private T InTransaction<T>(Func<IDbConnection, IDbTransaction, T> action)
    {
        using (var connection = _connectionFactory() ?? throw new InvalidOperationException("The connection factory returned no database connection."))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                var result = action(connection, transaction);
                transaction.Commit();
                return result;
            }
        }
    }

    private sealed class LineIdentity
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public string? BoxNo { get; set; }
        public bool IsRepeated { get; set; }
    }

    private sealed class DeleteChecks
    {
        public bool? IsClosed { get; set; }
        public bool? CanEdit { get; set; }
    }
}
