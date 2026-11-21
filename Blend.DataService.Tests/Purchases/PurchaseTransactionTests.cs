using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Blend.DataService.Mapping;
using Blend.DataService.Purchases;

namespace Blend.DataService.Tests.Purchases;

[TestClass]
public sealed class PurchaseTransactionTests
{
    [TestMethod]
    public void AddRepeatedLinePassesValuesAndCommitsAllSideEffects()
    {
        var connection = new FakeConnection();
        var service = Service(connection);
        var first = Line();
        var repeated = Line(); repeated.Packs = 2; repeated.Weight = 7; repeated.Value = 21;
        var box = service.AddLines(2, first, repeated, doc => $"{doc.DocNo:D5}{doc.Prefix}");
        Assert.AreEqual("00123P", box);
        Assert.IsTrue(connection.Transaction!.Committed);
        CollectionAssert.AreEqual(new[] { "trn_TinNoUpdate", "trn_PurchaseDetailInsert", "trn_TeaStockInsert", "trn_PurchaseDetailInsert", "trn_TeaStockInsert", "trn_DocumentNoGenerate", "trn_GardenMarkInsert", "trn_GradeInsert" }, connection.Commands);
        Assert.AreEqual("00123P", connection.Arguments[1]["BoxNos"]);
        Assert.AreEqual(string.Empty, connection.Arguments[1]["Standards"]);
        Assert.AreEqual(false, connection.Arguments[1]["Repeated"]);
        Assert.AreEqual(true, connection.Arguments[3]["Repeated"]);
        Assert.AreEqual(21m, connection.Arguments[3]["Price"]);
        Assert.AreEqual(42, connection.Arguments[2]["stItemId"]);
        Assert.AreEqual(7m, connection.Arguments[4]["stkQty"]);
        Assert.AreEqual(2, connection.Arguments[5]["CompanyId"]);
    }

    [TestMethod]
    public void LookupFailureRollsBackPurchaseAndDocumentIncrement()
    {
        var connection = new FakeConnection { FailCommand = "trn_GradeInsert" };
        Assert.ThrowsExactly<InvalidOperationException>(() => Service(connection).AddLines(2, Line(), null, _ => "BOX"));
        Assert.IsFalse(connection.Transaction!.Committed);
        Assert.IsTrue(connection.Transaction.RolledBack);
        Assert.IsTrue(connection.Commands.Contains("trn_DocumentNoGenerate"));
    }

    [TestMethod]
    public void UpdatePassesVersionAndRollsBackIfStockFails()
    {
        var connection = new FakeConnection { FailCommand = "trn_TeaStockUpdateByEditLine" };
        var first = Line(); first.Id = 1; first.StockId = 4; first.RowVersion = [1, 2, 3, 4, 5, 6, 7, 8];
        Assert.ThrowsExactly<InvalidOperationException>(() => Service(connection).UpdateLines(first, null));
        Assert.IsTrue(connection.Transaction!.RolledBack);
        CollectionAssert.AreEqual(new[] { "trn_PrurchaseDetailUpdate", "trn_TeaStockUpdateByEditLine" }, connection.Commands);
        CollectionAssert.AreEqual(first.RowVersion, (byte[])connection.Arguments[0]["RowVersion"]!);
        Assert.AreEqual(4L, connection.Arguments[0]["stockid"]);
    }

    [TestMethod]
    public void RepeatedUpdateRequiresBothLinesAndPassesEachVersion()
    {
        var first = Line(); first.Id = 1; first.StockId = 4; first.RowVersion = [1, 1, 1, 1, 1, 1, 1, 1];
        var second = Line(); second.Id = 2; second.StockId = 5; second.RowVersion = [2, 2, 2, 2, 2, 2, 2, 2];
        var incomplete = new FakeConnection { HasRepeated = true };
        Assert.ThrowsExactly<InvalidOperationException>(() => Service(incomplete).UpdateLines(first, null));
        Assert.IsTrue(incomplete.Transaction!.RolledBack);
        Assert.HasCount(0, incomplete.Commands);
        var connection = new FakeConnection { HasRepeated = true };
        Assert.IsTrue(Service(connection).UpdateLines(first, second));
        Assert.IsTrue(connection.Transaction!.Committed);
        CollectionAssert.AreEqual(new[] { "trn_PrurchaseDetailUpdate", "trn_TeaStockUpdateByEditLine", "trn_PrurchaseDetailUpdate", "trn_TeaStockUpdateByEditLine" }, connection.Commands);
        CollectionAssert.AreEqual(second.RowVersion, (byte[])connection.Arguments[2]["RowVersion"]!);
        Assert.AreEqual(5L, connection.Arguments[3]["stkId"]);
    }

    [TestMethod]
    public void InvalidFormattedNumberRollsBackBeforeWrites()
    {
        var connection = new FakeConnection();
        Assert.ThrowsExactly<InvalidOperationException>(() => Service(connection).AddLines(2, Line(), null, _ => "12345678901"));
        Assert.IsTrue(connection.Transaction!.RolledBack);
        Assert.HasCount(0, connection.Commands);
    }

    [TestMethod]
    public void DeleteScopesRepeatedRowsToSaleAndCommits()
    {
        var connection = new FakeConnection();
        var line = Line(); line.Id = 1; line.StockId = 4; line.BoxNo = "00123P";
        Assert.IsTrue(Service(connection).DeleteLine(line));
        Assert.IsTrue(connection.Transaction!.Committed);
        Assert.HasCount(1, connection.Commands);
        StringAssert.Contains(connection.Commands[0], "WHERE SaleId = @SaleId AND BoxNo = @BoxNo AND IsRepeated = 1");
        Assert.AreEqual(1, connection.Arguments[0]["SaleId"]);
    }

    [TestMethod]
    public void DeleteRejectsAllocatedStockBeforeWriting()
    {
        var connection = new FakeConnection { CanEdit = false };
        var line = Line(); line.Id = 1; line.StockId = 4; line.BoxNo = "00123P";
        Assert.ThrowsExactly<InvalidOperationException>(() => Service(connection).DeleteLine(line));
        Assert.IsTrue(connection.Transaction!.RolledBack);
        Assert.HasCount(0, connection.Commands);
    }

    private static PurchaseDataService Service(FakeConnection connection) => new(new UnusedMapper(), () => connection);
    private static PurchaseLineRecord Line() => new() { SaleId = 1, BrokerId = 3, User = "user", Grade = "A", GardenMark = "M", NetWeight = 2, Packs = 5, Weight = 10, ReducedQty = 0, Rate = 3, Value = 30, VirtualWeight = 10, VirtualValue = 30 };

    private sealed class UnusedMapper : IStoredProcedureMapper
    {
        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null) => throw new NotSupportedException();
        public int Execute(string procedureName, object? parameters = null) => throw new NotSupportedException();
    }

    private sealed class FakeConnection : IDbConnection
    {
        public List<string> Commands { get; } = [];
        public List<Dictionary<string, object?>> Arguments { get; } = [];
        public string? FailCommand { get; init; }
        public bool HasRepeated { get; init; }
        public bool CanEdit { get; init; } = true;
        public FakeTransaction? Transaction { get; private set; }
        [AllowNull] public string ConnectionString { get; set; } = string.Empty;
        public int ConnectionTimeout => 0;
        public string Database => "test";
        public ConnectionState State { get; private set; }
        public IDbTransaction BeginTransaction() => Transaction = new FakeTransaction(this);
        public IDbTransaction BeginTransaction(IsolationLevel il) => BeginTransaction();
        public void ChangeDatabase(string databaseName) { }
        public void Close() => State = ConnectionState.Closed;
        public IDbCommand CreateCommand() => new FakeCommand(this);
        public void Open() => State = ConnectionState.Open;
        public void Dispose() => Close();
    }

    private sealed class FakeTransaction(FakeConnection connection) : IDbTransaction
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }
        public IDbConnection Connection => connection;
        public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        public void Commit() => Committed = true;
        public void Rollback() => RolledBack = true;
        public void Dispose() { if (!Committed) Rollback(); }
    }

    private sealed class FakeCommand(FakeConnection connection) : IDbCommand
    {
        [AllowNull] public string CommandText { get; set; } = string.Empty;
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection? Connection { get; set; } = connection;
        public IDataParameterCollection Parameters { get; } = new FakeParameters();
        public IDbTransaction? Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }
        public void Cancel() { }
        public IDbDataParameter CreateParameter() => new FakeParameter();
        public void Dispose() { }
        public int ExecuteNonQuery()
        {
            Assert.AreSame(connection.Transaction, Transaction);
            connection.Commands.Add(CommandText);
            connection.Arguments.Add(Parameters.Cast<IDataParameter>().ToDictionary(p => p.ParameterName.TrimStart('@'), p => p.Value, StringComparer.OrdinalIgnoreCase));
            if (CommandText == connection.FailCommand) throw new InvalidOperationException("Simulated database error.");
            if (CommandText == "trn_PurchaseDetailInsert") ((IDbDataParameter)Parameters["lastId"]).Value = 42;
            return 1;
        }
        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            Assert.AreSame(connection.Transaction, Transaction);
            var table = new DataTable();
            if (CommandText.Contains("FROM purchase_header")) { table.Columns.Add("Id", typeof(int)); table.Rows.Add(1); }
            else if (CommandText.Contains("FROM documentno"))
            {
                table.Columns.Add("DocNo", typeof(int)); table.Columns.Add("DocFormat", typeof(string)); table.Columns.Add("Prefix", typeof(string)); table.Columns.Add("CompanyId", typeof(int)); table.Columns.Add("DocType", typeof(int)); table.Rows.Add(123, "00000", "P", 2, 1);
            }
            else if (CommandText.Contains("FROM Tin_No")) { table.Columns.Add("TinNo", typeof(int)); table.Rows.Add(1); }
            else if (CommandText.Contains("FROM purchase_detail p"))
            {
                table.Columns.Add("Id", typeof(long)); table.Columns.Add("BoxNo", typeof(string)); table.Columns.Add("IsRepeated", typeof(bool)); table.Columns.Add("StockId", typeof(long)); table.Rows.Add(1L, "00123P", false, 4L);
                if (connection.HasRepeated && CommandText.Contains("ORDER BY p.Id")) table.Rows.Add(2L, "00123P", true, 5L);
            }
            else if (CommandText.Contains("func_IsSaleClose")) { table.Columns.Add("IsClosed", typeof(bool)); table.Columns.Add("CanEdit", typeof(bool)); table.Rows.Add(false, connection.CanEdit); }
            else throw new InvalidOperationException("Unexpected query: " + CommandText);
            return table.CreateDataReader();
        }
        public object? ExecuteScalar() => throw new NotSupportedException();
        public void Prepare() { }
    }

    private sealed class FakeParameters : ArrayList, IDataParameterCollection
    {
        public object this[string parameterName] { get => this[IndexOf(parameterName)]!; set => this[IndexOf(parameterName)] = value; }
        public bool Contains(string parameterName) => IndexOf(parameterName) >= 0;
        public int IndexOf(string parameterName)
        {
            for (var i = 0; i < Count; i++) if (string.Equals(((IDataParameter)this[i]!).ParameterName.TrimStart('@'), parameterName.TrimStart('@'), StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }
        public void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));
    }

    private sealed class FakeParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public bool IsNullable => true;
        [AllowNull] public string ParameterName { get; set; } = string.Empty;
        [AllowNull] public string SourceColumn { get; set; } = string.Empty;
        public DataRowVersion SourceVersion { get; set; }
        public object? Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
}
