using System.Data;
using System.Reflection;
using Blend.DataService.Mapping;
using Blend.DataService.Reports.Purchase;
using Dapper;

namespace Blend.DataService.Tests.Reports.Purchase;

[TestClass]
public sealed class PurchaseReportDataServiceTests
{
    [TestMethod]
    public void QueriesUseLegacyProceduresAndParametersAndReturnMapperResults()
    {
        var mapper = new RecordingMapper();
        var service = new PurchaseReportDataService(mapper);

        var summaries = new[] { new SaleSummaryRecord { NoOfLots = 3 } };
        mapper.Result = summaries;
        Assert.AreSame(summaries, service.GetSaleSummary(17));
        Check<SaleSummaryRecord>(mapper, "rpt_SalesSummary", ("SaleId", 17));

        var sheets = new[] { new PurchaseSheetRecord { BoxNo = "B01" } };
        mapper.Result = sheets;
        Assert.AreSame(sheets, service.GetPurchaseSheet(17));
        Check<PurchaseSheetRecord>(mapper, "rpt_PurchaseSheetBySaleId", ("SaleId", 17));

        var canisters = new[] { new CanisterSheetRecord { TinNo = "T01" } };
        mapper.Result = canisters;
        Assert.AreSame(canisters, service.GetCanisterSheet(17));
        Check<CanisterSheetRecord>(mapper, "rpt_CanisterSheetBySaleId", ("SaleId", 17));

        mapper.Result = sheets;
        Assert.AreSame(sheets, service.GetPurchaseSheetByAdvanceFilter("2026-01-01", "2026-02-01", "G", "S", "Auction", "B"));
        Check<PurchaseSheetRecord>(mapper, "rpt_PurchaseSheetBySaleDate", ("FromSaleDate", "2026-01-01"), ("ToSaleDate", "2026-02-01"), ("Standard", "S"), ("Grade", "G"), ("SaleType", "Auction"), ("BrokerCode", "B"));
        Assert.AreSame(sheets, service.GetPurchaseSheetByBroker("2026-01-01", "2026-02-01", 7));
        Check<PurchaseSheetRecord>(mapper, "rpt_PurchaseSheetByBroker", ("FromSaleDate", "2026-01-01"), ("ToSaleDate", "2026-02-01"), ("BrokerId", 7));

        service.GetPurchaseSheetByAdvanceFilter(null, null, null, null, null, null);
        Check<PurchaseSheetRecord>(mapper, "rpt_PurchaseSheetBySaleDate", ("FromSaleDate", null), ("ToSaleDate", null), ("Standard", null), ("Grade", null), ("SaleType", null), ("BrokerCode", null));
    }

    [TestMethod]
    public void DatabaseErrorsPropagate()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PurchaseReportDataService(null!));
        var failure = new InvalidOperationException("Database failure");
        var mapper = new RecordingMapper { Failure = failure };
        var service = new PurchaseReportDataService(mapper);
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetSaleSummary(1)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheet(1)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetCanisterSheet(1)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheetByAdvanceFilter(null, null, null, null, null, null)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPurchaseSheetByBroker(null, null, 1)));
    }

    [TestMethod]
    public void ReportRecordsMapAllNamedColumnsWithDapper()
    {
        CheckMapping<SaleSummaryRecord>();
        CheckMapping<PurchaseSheetRecord>();
        CheckMapping<CanisterSheetRecord>();
    }

    [TestMethod]
    public void DapperConvertsLegacyMappedNumericColumns()
    {
        var summaryTable = new DataTable();
        summaryTable.Columns.Add("NinetyPercVal", typeof(string));
        summaryTable.Columns.Add("TenPercVal", typeof(string));
        summaryTable.Columns.Add("HundredPercVal", typeof(string));
        summaryTable.Rows.Add("90", "10", "100");
        using var summaryReader = summaryTable.CreateDataReader();
        Assert.IsTrue(summaryReader.Read());
        var summary = summaryReader.GetRowParser<SaleSummaryRecord>()(summaryReader);
        Assert.AreEqual(90m, summary.NinetyPercVal);
        Assert.AreEqual(10m, summary.TenPercVal);
        Assert.AreEqual(100m, summary.HundredPercVal);

        var sheetTable = new DataTable();
        sheetTable.Columns.Add("IsRepeated", typeof(decimal));
        sheetTable.Rows.Add(1m);
        using var sheetReader = sheetTable.CreateDataReader();
        Assert.IsTrue(sheetReader.Read());
        Assert.IsTrue(sheetReader.GetRowParser<PurchaseSheetRecord>()(sheetReader).IsRepeated);
    }

    private static void CheckMapping<T>() where T : class
    {
        var properties = typeof(T).GetProperties();
        var table = new DataTable();
        foreach (var property in properties) table.Columns.Add(property.Name, property.PropertyType);
        var values = properties.Select((property, index) => property.PropertyType == typeof(string) ? (object)$"value-{index}" :
            property.PropertyType == typeof(decimal) ? 12.75m + index :
            property.PropertyType == typeof(int) ? index + 1 :
            property.PropertyType == typeof(bool) ? true : throw new InvalidOperationException($"Unexpected type {property.PropertyType}")).ToArray();
        table.Rows.Add(values);
        using var reader = table.CreateDataReader();
        Assert.IsTrue(reader.Read());
        var record = reader.GetRowParser<T>()(reader);
        for (var index = 0; index < properties.Length; index++)
            Assert.AreEqual(values[index], properties[index].GetValue(record), properties[index].Name);
    }

    private static void Check<T>(RecordingMapper mapper, string procedure, params (string Name, object? Value)[] parameters)
    {
        Assert.AreEqual(procedure, mapper.Procedure);
        Assert.AreEqual(typeof(T), mapper.ResultType);
        var properties = mapper.Parameters!.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.AreEqual(parameters.Length, properties.Length);
        foreach (var (name, value) in parameters)
            Assert.AreEqual(value, properties.Single(property => property.Name == name).GetValue(mapper.Parameters), name);
    }

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? Procedure { get; private set; }
        public object? Parameters { get; private set; }
        public Type? ResultType { get; private set; }
        public object? Result { get; set; }
        public Exception? Failure { get; set; }

        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null)
        {
            if (Failure is not null) throw Failure;
            Procedure = procedureName;
            Parameters = parameters;
            ResultType = typeof(T);
            return (IReadOnlyList<T>)Result!;
        }

        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public int Execute(string procedureName, object? parameters = null) => throw new NotSupportedException();
    }
}
