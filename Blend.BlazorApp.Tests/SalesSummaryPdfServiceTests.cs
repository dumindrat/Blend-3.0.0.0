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
        Assert.AreEqual(400000m, totals.HundredPercent);
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
        using var document = PdfDocument.Open(renderer.Generate("Test Company", rows));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        foreach (var expected in new[] { "Test Company", "2030-07-04", "Test Broker One", "Test Broker Two", "41", "42", "EA", "PB", "400.00", "7,000.00", "17.50", "6,300.00", "700.00" })
            StringAssert.Contains(text, expected);
    }

    [TestMethod]
    public void ParameterizedPdfRejectsInvalidRowsAndHandlesZeroWeight()
    {
        var renderer = new SalesSummaryPdfService(new TestEnvironment(Environments.Development));
        Assert.ThrowsExactly<ArgumentNullException>(() => renderer.Generate("Company", null!));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", []));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [new()]));
        Assert.ThrowsExactly<ArgumentException>(() => renderer.Generate("Company", [new() { SaleDate = "2030-01-01" }, new() { SaleDate = "2030-01-02" }]));
        using var document = PdfDocument.Open(renderer.Generate(null, [new() { SaleDate = "2030-01-01", Weight = 0, Value = 12 }]));
        var text = string.Join("\n", document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
        StringAssert.Contains(text, "Sale average: N/A");
        Assert.ThrowsExactly<InvalidOperationException>(() => new SalesSummaryPdfService(new TestEnvironment(Environments.Production)).Generate("Company", [new() { SaleDate = "2030-01-01" }]));
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
