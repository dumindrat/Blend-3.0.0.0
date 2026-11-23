using System.Globalization;
using Blend.BusinessService.Reports.Purchase;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Blend.BlazorApp.Services;

public sealed class SalesSummaryPdfService(IHostEnvironment environment)
{
    public byte[] Generate(string? companyName, IReadOnlyList<SaleSummary> rows, string printedBy, DateTimeOffset printedAt)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("The QuestPDF evaluation report is available only in Development.");
        ArgumentException.ThrowIfNullOrWhiteSpace(printedBy);
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
            page.Content().Column(content =>
            {
                content.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3.1f);
                        columns.RelativeColumn(0.7f);
                        columns.RelativeColumn(0.7f);
                        for (var i = 0; i < 4; i++) columns.RelativeColumn(1.2f);
                    });
                    table.Header(header =>
                    {
                        foreach (var title in new[] { "Broker", "Type", "Lots" })
                            header.Cell().Element(HeaderCell).Text(title).Bold();
                        foreach (var title in new[] { "Weight", "Value", "90%", "10%" })
                            header.Cell().Element(HeaderCell).AlignRight().Text(title).Bold();
                    });
                    foreach (var row in rows)
                    {
                        table.Cell().Element(Cell).Text(row.BrokerName ?? "");
                        table.Cell().Element(Cell).Text(row.SaleType ?? "");
                        table.Cell().Element(Cell).Text(row.NoOfLots.ToString(CultureInfo.InvariantCulture));
                        foreach (var value in new[] { row.Weight, row.Value, row.NinetyPercVal, row.TenPercVal })
                            table.Cell().Element(Cell).AlignRight().Text(FormatNumber(value));
                    }
                    table.Cell().Element(HeaderCell).Text("Total").Bold();
                    table.Cell().Element(HeaderCell).Text("");
                    table.Cell().Element(HeaderCell).Text(totals.Lots.ToString(CultureInfo.InvariantCulture)).Bold();
                    foreach (var value in new[] { totals.Weight, totals.Value, totals.NinetyPercent, totals.TenPercent })
                        table.Cell().Element(HeaderCell).AlignRight().Text(FormatNumber(value)).Bold();
                });
                content.Item().PaddingTop(10).AlignLeft().Width(320).Border(1).BorderColor(Colors.Grey.Darken1).Padding(8).Column(summary =>
                {
                    summary.Item().PaddingBottom(4).Text("Summary").Bold().FontSize(11);
                    AddSummaryRow(summary, "No Of Lots:", totals.Lots.ToString(CultureInfo.InvariantCulture));
                    AddSummaryRow(summary, "Total Sale Qty:", FormatNumber(totals.Weight));
                    AddSummaryRow(summary, "Total Value:", FormatNumber(totals.Value));
                    AddSummaryRow(summary, "Total 10% of Adv:", FormatNumber(totals.TenPercent));
                    AddSummaryRow(summary, "Total 90% Bal:", FormatNumber(totals.NinetyPercent));
                    AddSummaryRow(summary, "Sale Avg:", FormatAverage(totals.Average));
                });
            });
            page.Footer().Row(footer =>
            {
                footer.RelativeItem().Text($"Printed by: {printedBy} | Printed at: {printedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} UTC");
                footer.ConstantItem(60).AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
            });
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
            weight == 0 ? null : value / weight);
    }

    private static void AddSummaryRow(ColumnDescriptor summary, string label, string value) =>
        summary.Item().PaddingVertical(2).Row(row =>
        {
            row.ConstantItem(130).Text(label).Bold();
            row.RelativeItem().Text(value);
        });

    private static string FormatNumber(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
    private static string FormatAverage(decimal? value) => value is null ? "N/A" : FormatNumber(value.Value);
    private static IContainer Cell(IContainer container) => container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Grey.Lighten3).Padding(4);
}

public sealed record SummaryTotals(int Lots, decimal Weight, decimal Value, decimal NinetyPercent, decimal TenPercent, decimal? Average);
