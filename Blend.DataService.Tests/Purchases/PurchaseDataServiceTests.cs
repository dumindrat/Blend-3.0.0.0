using System.Reflection;
using Blend.DataService.Mapping;
using Blend.DataService.Purchases;

namespace Blend.DataService.Tests.Purchases;

[TestClass]
public sealed class PurchaseDataServiceTests
{
    [TestMethod]
    public void ReadOperationsUseDatabaseParameterNames()
    {
        var mapper = new RecordingMapper();
        var service = new PurchaseDataService(mapper, () => throw new InvalidOperationException());
        service.GetSales("2026", 2);
        Check(mapper, "trn_PurchaseHeaderGet", ("serachString", "2026"), ("CompanyId", 2));
        service.GetLinesBySale(17);
        Check(mapper, "trn_PurchaseDetailGetBySaleId", ("Sale", 17));
        service.GetLinesByReservedNo(55);
        Check(mapper, "trn_PurchaseDetailGetByReservedNoId", ("ReservedNoId", 55L));
        service.GetTeaCertificates();
        Assert.AreEqual("trn_TeaCertificateGet", mapper.Procedure);
        Assert.IsNull(mapper.Parameters);
    }

    [TestMethod]
    public void SaleWritesUseLegacyProcedureParameters()
    {
        var mapper = new RecordingMapper();
        var service = new PurchaseDataService(mapper, () => throw new InvalidOperationException());
        var sale = new PurchaseSaleRecord { Id = 17, CompanyId = 2, SaleDate = "2026-11-20", User = "user" };
        Assert.IsTrue(service.CreateSale(sale));
        Check(mapper, "trn_PurchaseHeaderInsert", ("SaleDates", sale.SaleDate), ("User", sale.User), ("CompanyId", 2));
        Assert.IsTrue(service.UpdateSale(sale));
        Check(mapper, "trn_PurchaseHeaderUpdate", ("SaleId", 17), ("SaleDate", sale.SaleDate), ("User", sale.User));
        Assert.IsTrue(service.SetSaleClosed(sale, true));
        Check(mapper, "trn_PurchaseHeaderUpdateByClose", ("SaleId", 17), ("Status", true), ("User", sale.User));
        Assert.IsTrue(service.SetSaleClosed(sale, false));
        Check(mapper, "trn_PurchaseHeaderUpdateByClose", ("SaleId", 17), ("Status", false), ("User", sale.User));
    }

    [TestMethod]
    public void ErrorsAndInvalidArgumentsAreNotSwallowed()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PurchaseDataService(null!, () => throw new InvalidOperationException()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new PurchaseDataService(new RecordingMapper(), null!));
        var mapper = new RecordingMapper();
        var service = new PurchaseDataService(mapper, () => throw new InvalidOperationException());
        Assert.ThrowsExactly<ArgumentNullException>(() => service.CreateSale(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => service.UpdateLines(null!, null));
        Assert.ThrowsExactly<ArgumentException>(() => service.UpdateLines(new PurchaseLineRecord(), null));
        Assert.ThrowsExactly<ArgumentException>(() => service.DeleteLine(new PurchaseLineRecord()));
        var failure = new InvalidOperationException("Database failure");
        mapper.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.GetSales("", 1)));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => service.UpdateSale(new PurchaseSaleRecord())));
    }

    private static void Check(RecordingMapper mapper, string procedure, params (string Name, object? Value)[] parameters)
    {
        Assert.AreEqual(procedure, mapper.Procedure);
        var properties = mapper.Parameters!.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.AreEqual(parameters.Length, properties.Length);
        foreach (var (name, value) in parameters)
            Assert.AreEqual(value, properties.Single(property => property.Name == name).GetValue(mapper.Parameters), name);
    }

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? Procedure { get; private set; }
        public object? Parameters { get; private set; }
        public Exception? Failure { get; set; }
        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null) { Record(procedureName, parameters); return []; }
        public int Execute(string procedureName, object? parameters = null) { Record(procedureName, parameters); return -1; }
        private void Record(string procedure, object? parameters)
        {
            if (Failure is not null) throw Failure;
            Procedure = procedure;
            Parameters = parameters;
        }
    }
}
