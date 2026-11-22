using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Blend.BlazorApp.Tests;

internal sealed class SalesSummaryTestConnection : IDbConnection
{
    public bool OwnsSale { get; set; } = true;
    public TestTransaction? Transaction { get; private set; }
    public Dictionary<string, object?> Arguments { get; } = new(StringComparer.OrdinalIgnoreCase);
    [AllowNull] public string ConnectionString { get; set; } = "";
    public int ConnectionTimeout => 0;
    public string Database => "test";
    public ConnectionState State { get; private set; }
    public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted);
    public IDbTransaction BeginTransaction(IsolationLevel level) => Transaction = new TestTransaction(this, level);
    public void ChangeDatabase(string databaseName) => throw new NotSupportedException();
    public void Close() => State = ConnectionState.Closed;
    public IDbCommand CreateCommand() => new TestCommand(this);
    public void Open() => State = ConnectionState.Open;
    public void Dispose() => Close();

    internal sealed class TestTransaction(SalesSummaryTestConnection connection, IsolationLevel level) : IDbTransaction
    {
        public bool Committed { get; private set; }
        public bool Disposed { get; private set; }
        public IDbConnection Connection => connection;
        public IsolationLevel IsolationLevel => level;
        public void Commit() => Committed = true;
        public void Rollback() { }
        public void Dispose() => Disposed = true;
    }

    private sealed class TestCommand(SalesSummaryTestConnection connection) : IDbCommand
    {
        [AllowNull] public string CommandText { get; set; } = "";
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection? Connection { get; set; } = connection;
        public IDataParameterCollection Parameters { get; } = new TestParameters();
        public IDbTransaction? Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }
        public void Cancel() { }
        public IDbDataParameter CreateParameter() => new TestParameter();
        public void Dispose() { }
        public int ExecuteNonQuery() => throw new NotSupportedException();
        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            Assert.AreSame(connection.Transaction, Transaction);
            StringAssert.Contains(CommandText, "FROM purchase_header WITH (HOLDLOCK)");
            foreach (IDataParameter parameter in Parameters)
                connection.Arguments[parameter.ParameterName.TrimStart('@')] = parameter.Value;
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            if (connection.OwnsSale) table.Rows.Add(connection.Arguments["SaleId"]);
            return table.CreateDataReader();
        }
        public object? ExecuteScalar() => throw new NotSupportedException();
        public void Prepare() { }
    }

    private sealed class TestParameters : ArrayList, IDataParameterCollection
    {
        public object this[string name] { get => this[IndexOf(name)]!; set => this[IndexOf(name)] = value; }
        public bool Contains(string name) => IndexOf(name) >= 0;
        public int IndexOf(string name)
        {
            for (var i = 0; i < Count; i++)
                if (string.Equals(((IDataParameter)this[i]!).ParameterName.TrimStart('@'), name.TrimStart('@'), StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }
        public void RemoveAt(string name) => RemoveAt(IndexOf(name));
    }

    private sealed class TestParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public bool IsNullable => true;
        [AllowNull] public string ParameterName { get; set; } = "";
        [AllowNull] public string SourceColumn { get; set; } = "";
        public DataRowVersion SourceVersion { get; set; }
        public object? Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
}
