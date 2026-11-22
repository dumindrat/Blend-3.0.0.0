using System.Globalization;
using Blend.BusinessService.Reports.Purchase;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Blend.BlazorApp.Services;

public sealed class SalesSummaryPdfService(IHostEnvironment environment)
{
    public byte[] Generate(string? companyName, IReadOnlyList<SaleSummary> rows)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("The QuestPDF evaluation report is available only in Development.");
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
            throw new ArgumentException("The sales summary contains no data.", nameof(rows));
        if (rows.Any(row => row is null || string.IsNullOrWhiteSpace(row.SaleDate)))
            throw new ArgumentException("Every sales summary row must have a sale date.", nameof(rows));
        var saleDate = rows[0].SaleDate!;
        if (rows.Any(row => !string.Equals(row.SaleDate, saleDate, StringComparison.Ordinal)))
            throw new ArgumentException("Sales summary rows must have the same sale date.", nameof(rows));

        QuestPDF.Settings.License = LicenseType.Evaluation;
        var totals = CalculateTotals(rows);

        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.DefaultTextStyle(style => style.FontSize(9));
            page.Header().Column(header =>
            {
                header.Item().Text("Sales Summary - " + (companyName ?? "")).Bold().FontSize(15);
                header.Item().PaddingBottom(8).Text("Sale date: " + saleDate + " | Sale average: " + FormatAverage(totals.Average));
            });
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.4f);
                    columns.RelativeColumn(0.7f);
                    columns.RelativeColumn(0.7f);
                    columns.RelativeColumn(0.7f);
                    for (var i = 0; i < 5; i++) columns.RelativeColumn(1.2f);
                });
                table.Header(header =>
                {
                    foreach (var title in new[] { "Broker", "ID", "Type", "Lots", "Weight", "Value", "90%", "10%", "100%" })
                        header.Cell().Element(HeaderCell).Text(title).Bold();
                });
                foreach (var row in rows)
                {
                    table.Cell().Element(Cell).Text(row.BrokerName ?? "");
                    table.Cell().Element(Cell).Text(row.BrokerId.ToString(CultureInfo.InvariantCulture));
                    table.Cell().Element(Cell).Text(row.SaleType ?? "");
                    table.Cell().Element(Cell).Text(row.NoOfLots.ToString(CultureInfo.InvariantCulture));
                    foreach (var value in new[] { row.Weight, row.Value, row.NinetyPercVal, row.TenPercVal, row.HundredPercVal })
                        table.Cell().Element(Cell).AlignRight().Text(FormatNumber(value));
                }
                table.Cell().Element(HeaderCell).Text("Total").Bold();
                table.Cell().Element(HeaderCell).Text("");
                table.Cell().Element(HeaderCell).Text("");
                table.Cell().Element(HeaderCell).Text(totals.Lots.ToString(CultureInfo.InvariantCulture)).Bold();
                foreach (var value in new[] { totals.Weight, totals.Value, totals.NinetyPercent, totals.TenPercent, totals.HundredPercent })
                    table.Cell().Element(HeaderCell).AlignRight().Text(FormatNumber(value)).Bold();
            });
            page.Footer().AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
        })).GeneratePdf();
    }

    public static SummaryTotals CalculateTotals(IEnumerable<SaleSummary> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var items = rows.ToArray();
        var weight = items.Sum(row => row.Weight);
        var value = items.Sum(row => row.Value);
        return new(items.Sum(row => row.NoOfLots), weight, value,
            items.Sum(row => row.NinetyPercVal), items.Sum(row => row.TenPercVal),
            items.Sum(row => row.HundredPercVal), weight == 0 ? null : value / weight);
    }

    private static string FormatNumber(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
    private static string FormatAverage(decimal? value) => value is null ? "N/A" : FormatNumber(value.Value);
    private static IContainer Cell(IContainer container) => container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Grey.Lighten3).Padding(4);
}

public sealed record SummaryTotals(int Lots, decimal Weight, decimal Value, decimal NinetyPercent, decimal TenPercent, decimal HundredPercent, decimal? Average);
