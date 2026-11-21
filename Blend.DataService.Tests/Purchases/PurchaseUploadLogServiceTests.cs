using Blend.DataService.Mapping;
using Blend.DataService.Purchases;

namespace Blend.DataService.Tests.Purchases;

[TestClass]
public sealed class PurchaseUploadLogServiceTests
{
    [TestMethod]
    public void InsertPassesLogAndPropagatesFailures()
    {
        var mapper = new RecordingMapper();
        var service = new PurchaseUploadLogService(mapper);
        var log = new PurchaseUploadLog { FileName = "purchase.csv", SaleId = 12, BrokerId = 3, ErrorRecords = 1, TotalRecords = 2, SaleCode = "A", SaleType = "MS", CreateUser = "operator", UploadData = "[]" };
        service.Insert(log);
        Assert.AreEqual("trn_SaleUploadLogInsert", mapper.Procedure);
        Assert.AreSame(log, mapper.Parameters);
        var failure = new InvalidOperationException("Database failure");
        mapper.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.Insert(log)));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.Insert(null!));
    }

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? Procedure { get; set; }
        public object? Parameters { get; set; }
        public Exception? Failure { get; set; }
        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null) => throw new NotSupportedException();
        public int Execute(string procedureName, object? parameters = null)
        {
            if (Failure is not null) throw Failure;
            Procedure = procedureName; Parameters = parameters; return -1;
        }
    }
}
