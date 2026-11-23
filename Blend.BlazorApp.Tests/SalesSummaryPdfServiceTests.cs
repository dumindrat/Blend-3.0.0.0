using Blend.BlazorApp.Services;
using Blend.BusinessService.Reports.Purchase;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class SalesSummaryPdfServiceTests
{
    private static readonly DateTimeOffset PrintedAt = new(2031, 2, 14, 12, 34, 56, TimeSpan.Zero);

    [TestMethod]
    public void TotalsUseWeightedAverageAndAllLegacyValueFields()
    {
        SaleSummary[] rows =
        [
            new() { NoOfLots = 8, Weight = 1000m, Value = 250000m, NinetyPercVal = 225000m, TenPercVal = 25000m, HundredPercVal = 250000m },
            new() { NoOfLots = 4, Weight = 500m, Value = 150000m, NinetyPercVal = 135000m, TenPercVal = 15000m, HundredPercVal = 150000m }
        ];
        var totals = SalesSummaryPdfService.CalculateTotals(rows);
        Assert.AreEqual(12, totals.Lots);
        Assert.AreEqual(1500m, totals.Weight);
        Assert.AreEqual(400000m, totals.Value);
        Assert.AreEqual(360000m, totals.NinetyPercent);
        Assert.AreEqual(40000m, totals.TenPercent);
        Assert.AreEqual(400000m / 1500m, totals.Average);
    }

    [TestMethod]
    public void EmptyAndZeroWeightHaveNoAverage()
    {
        Assert.IsNull(SalesSummaryPdfService.CalculateTotals([]).Average);
        Assert.IsNull(SalesSummaryPdfService.CalculateTotals([new() { Weight = 0, Value = 10 }]).Average);
    }

    [TestMethod]
    public void ParameterizedPdfUsesProvidedCompanyRowsAndWeightedAverage()
    {
        SaleSummary[] rows =
        [
            new() { SaleDate = "2030-07-04", BrokerId = 41, BrokerName = "Test Broker One", SaleType = "EA", NoOfLots = 2, Weight = 100, Value = 1000, NinetyPercVal = 900, TenPercVal = 100, HundredPercVal = 1000 },
            new() { SaleDate = "2030-07-04", BrokerId = 42, BrokerName = "Test Broker Two", SaleType = "PB", NoOfLots = 3, Weight = 300, Value = 6000, NinetyPercVal = 5400, TenPercVal = 600, HundredPercVal = 6000 }
        ];
        var renderer = new SalesSummaryPdfService(new TestEnvironment(Environments.Development));
        using var document = PdfDocument.Open(renderer.Generate("Test Company", rows, "test-user", PrintedAt));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        foreach (var expected in new[] { "Test Company", "2030-07-04", "Test Broker One", "Test Broker Two", "EA", "PB", "400.00", "7,000.00", "17.50", "6,300.00", "No Of Lots:", "5", "Total Sale Qty:", "400.00", "Total Value:", "7,000.00", "Total 10% of Adv:", "700.00", "Total 90% Bal:", "6,300.00", "Sale Avg:", "17.50" })
            StringAssert.Contains(text, expected);
        Assert.IsFalse(text.Contains("100%", StringComparison.Ordinal));
    }

    [TestMethod]
    public void PdfHeadersAlignWithTheirColumnValues()
    {
        SaleSummary[] rows = [new() { SaleDate = "2030-07-04", BrokerId = 41, BrokerName = "TEABROKER", SaleType = "ZZ", NoOfLots = 3, Weight = 123.45m, Value = 234.56m, NinetyPercVal = 345.67m, TenPercVal = 456.78m, HundredPercVal = 567.89m }];
        var renderer = new SalesSummaryPdfService(new TestEnvironment(Environments.Development));
        using var document = PdfDocument.Open(renderer.Generate("Company", rows, "test-user", PrintedAt));
        var words = document.GetPage(1).GetWords().ToArray();
        foreach (var (header, value) in new[] { ("Broker", "TEABROKER"), ("Type", "ZZ"), ("Lots", "3") })
        {
            var heading = words.Single(word => word.Text == header);
            var data = words.First(word => word.Text == value);
            Assert.IsTrue(Math.Abs(heading.BoundingBox.Left - data.BoundingBox.Left) < 1.5, $"{header} should be left-aligned with {value}.");
        }
        foreach (var (header, value) in new[] { ("Weight", "123.45"), ("Value", "234.56"), ("90%", "345.67"), ("10%", "456.78") })
        {
            var heading = words.Where(word => word.Text == header).OrderByDescending(word => word.BoundingBox.Bottom).First();
            var data = words.First(word => word.Text == value);
            Assert.IsTrue(Math.Abs(heading.BoundingBox.Right - data.BoundingBox.Right) < 1.5, $"{header} should be right-aligned with {value}.");
        }
        Assert.IsFalse(words.Any(word => word.Text == "100%"));
        Assert.IsFalse(words.Any(word => word.Text == "567.89"));
        Assert.IsFalse(words.Any(word => word.Text == "ID"));
        Assert.IsFalse(words.Any(word => word.Text == "41"));
    }

    [TestMethod]
    public void FooterDisplaysPrintUserAndUtcTimeOnEveryPage()
    {
        var rows = Enumerable.Range(1, 150).Select(i => new SaleSummary { SaleDate = "2030-01-01", BrokerName = $"Broker {i}", NoOfLots = 1, Weight = 10, Value = 20 }).ToArray();
        var renderer = new SalesSummaryPdfService(new TestEnvironment(Environments.Development));
        using var document = PdfDocument.Open(renderer.Generate("Company", rows, "alice@example.com", PrintedAt));
        Assert.IsGreaterThan(1, document.NumberOfPages);
        foreach (var page in document.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            StringAssert.Contains(text, "Printed by: alice@example.com");
            StringAssert.Contains(text, "Printed at: 2031-02-14 12:34:56 UTC");
            StringAssert.Contains(text, $"Page {page.Number}");
        }
        var finalPageText = ContentOrderTextExtractor.GetText(document.GetPage(document.NumberOfPages));
        foreach (var label in new[] { "No Of Lots:", "Total Sale Qty:", "Total Value:", "Total 10% of Adv:", "Total 90% Bal:", "Sale Avg:" })
            StringAssert.Contains(finalPageText, label);
    }

    [TestMethod]
    public void ParameterizedPdfRejectsInvalidRowsAndHandlesZeroWeight()
    {
        var renderer = new SalesSummaryPdfService(new TestEnvironment(Environments.Development));
        Assert.ThrowsExactly<ArgumentNullException>(() => renderer.Generate("Company", null!, "user", PrintedAt));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [], "user", PrintedAt));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [new()], "user", PrintedAt));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [new() { SaleDate = "2030-01-01" }, new() { SaleDate = "2030-01-02" }], "user", PrintedAt));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [new() { SaleDate = "2030-01-01" }], " ", PrintedAt));
        using var document = PdfDocument.Open(renderer.Generate(null, [new() { SaleDate = "2030-01-01", Weight = 0, Value = 12 }], "user", PrintedAt));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        StringAssert.Contains(text, "Sale average: N/A");
        Assert.ThrowsExactly<InvalidOperationException>(() => new SalesSummaryPdfService(new TestEnvironment(Environments.Production)).Generate("Company", [new() { SaleDate = "2030-01-01" }], "user", PrintedAt));
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
