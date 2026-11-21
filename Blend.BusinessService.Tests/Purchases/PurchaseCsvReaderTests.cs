using System.Text;
using Blend.BusinessService.Purchases;

namespace Blend.BusinessService.Tests.Purchases;

[TestClass]
public sealed class PurchaseCsvReaderTests
{
    [TestMethod]
    public void ReadsLegacyFourteenColumnFileAndQuotedValues()
    {
        using var stream = Data("header\n0,LOT,2,\"Garden, East\",INV,BOP,10,5,8,48,10,11,2,13\n");
        var row = PurchaseCsvReader.Read(stream).Single();
        Assert.AreEqual(2L, row.RowNumber);
        Assert.AreEqual("Garden, East", row.Mark);
        Assert.AreEqual(10m, row.Packs);
        Assert.AreEqual(5m, row.Net);
        Assert.AreEqual(48m, row.Quantity);
        Assert.AreEqual(2m, row.Reduction);
        Assert.AreEqual(2m, row.Rate);
    }

    [TestMethod]
    [DataRow("header\n")]
    [DataRow("header\n1,2,3\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,10,5,8,51,10,11,2,13\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,10.123,5,8,48,10,11,2,13\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,bad,5,8,48,10,11,2,13\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,1.01,1.01,8,1,10,11,2,13\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,10,5,8,47.99,10,11,0.01,13\n")]
    [DataRow("header\n0,LOT,2,MARK,INV,BOP,9999999999999999,9999999999999999,8,1,10,11,2,13\n")]
    public void RejectsInvalidRowsBeforeImport(string csv)
    {
        using var stream = Data(csv);
        Assert.ThrowsExactly<InvalidDataException>(() => PurchaseCsvReader.Read(stream));
    }

    [TestMethod]
    public void EnforcesMaximumRowCount()
    {
        const string row = "0,LOT,2,MARK,INV,BOP,10,5,8,48,10,11,2,13\n";
        using var allowed = Data("header\n" + string.Concat(Enumerable.Repeat(row, 5000)));
        Assert.AreEqual(5000, PurchaseCsvReader.Read(allowed).Count);
        using var tooMany = Data("header\n" + string.Concat(Enumerable.Repeat(row, 5001)));
        Assert.ThrowsExactly<InvalidDataException>(() => PurchaseCsvReader.Read(tooMany));
    }

    private static MemoryStream Data(string text) => new(Encoding.UTF8.GetBytes(text));
}
