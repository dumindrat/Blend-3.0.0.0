using System.Globalization;
using Blend.BlazorApp.Authentication;
using Blend.BusinessService.Reports.Purchase;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Blend.BlazorApp.Services;

public sealed class PurchaseSheetPdfService(IHostEnvironment environment)
{
    public byte[] Generate(int saleId, string? companyName, IReadOnlyList<PurchaseSheet> rows, string printedBy, DateTimeOffset printedAt, string dateFormat, bool withValues)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("The QuestPDF evaluation report is available only in Development.");
        ArgumentException.ThrowIfNullOrWhiteSpace(printedBy);
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0) throw new ArgumentException("The purchase sheet contains no data.", nameof(rows));
        if (!CompanyDateFormats.IsSupported(dateFormat)) throw new ArgumentOutOfRangeException(nameof(dateFormat));

        QuestPDF.Settings.License = LicenseType.Evaluation;
        var headers = withValues
            ? new[] { "No", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade", "Packs", "Net Weight", "Weight", "Rate", "Value" }
            : new[] { "No", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade" };
        var totalLots = rows.Select(row => row.LotNo?.Trim()).Where(lot => !string.IsNullOrEmpty(lot)).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var totalChests = rows.Sum(row => row.Packs);
        var totalWeight = rows.Sum(row => row.VirtualWeight);
        var totalValue = rows.Sum(row => row.VirtualValue);
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A3.Landscape());
            page.Margin(24);
            page.DefaultTextStyle(style => style.FontSize(withValues ? 8 : 10));
            page.Header().Border(1).BorderColor(Colors.Grey.Darken1).PaddingVertical(14).PaddingHorizontal(12).Column(header =>
            {
                header.Item().Text("Purchase Sheet - " + (companyName ?? string.Empty)).Bold().FontSize(15);
                header.Item().PaddingBottom(4).Text("Sale date: " + CompanyDateFormats.Format(rows[0].SaleDate, dateFormat));
                if (withValues)
                {
                    header.Item().PaddingTop(4).Text("Purchase Totals").Bold();
                    AddSummaryRow(header, "No of Lots :", totalLots.ToString(CultureInfo.InvariantCulture));
                    AddSummaryRow(header, "Tot No of Chests :", Number(totalChests));
                    AddSummaryRow(header, "Total Purchase Qty :", Number(totalWeight));
                    AddSummaryRow(header, "Total Purchase Value :", Number(totalValue));
                }
            });
            page.Content().Column(content =>
            {
                content.Item().PaddingTop(20).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var index = 0; index < headers.Length; index++)
                    {
                        var width = index switch
                        {
                            0 or 3 or 4 or 5 => 0.4f,
                            1 or 6 or 7 or 10 => 0.6f,
                            2 or 11 or 12 => 0.7f,
                            8 => 1.6f,
                            9 => 0.8f,
                            _ => 1f
                        };
                        columns.RelativeColumn(width);
                    }
                });
                table.Header(row =>
                {
                    for (var index = 0; index < headers.Length; index++)
                    {
                        if (withValues && index >= 11)
                            row.Cell().Element(HeaderCell).AlignRight().Text(headers[index]).Bold();
                        else
                            row.Cell().Element(HeaderCell).Text(headers[index]).Bold();
                    }
                });
                for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    table.Cell().Element(Cell).Text((rowIndex + 1).ToString(CultureInfo.InvariantCulture));
                    foreach (var cell in GetCells(rows[rowIndex], dateFormat, withValues))
                    {
                        if (cell.Numeric) table.Cell().Element(Cell).AlignRight().Text(cell.Value);
                        else table.Cell().Element(Cell).Text(cell.Value);
                    }
                }
                if (withValues)
                {
                    for (var index = 0; index < headers.Length; index++)
                    {
                        if (index == 5)
                            table.Cell().Element(HeaderCell).Text("Total").Bold();
                        else if (index == 13)
                            table.Cell().Element(HeaderCell).AlignRight().Text(Number(totalWeight)).Bold();
                        else if (index == 15)
                            table.Cell().Element(HeaderCell).AlignRight().Text(Number(totalValue)).Bold();
                        else
                            table.Cell().Element(HeaderCell).Text("");
                    }
                }
                });
            });
            page.Footer().Row(footer =>
            {
                footer.RelativeItem().Text($"Printed by: {printedBy} | Printed at: {printedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} UTC");
                footer.ConstantItem(60).AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
            });
        })).GeneratePdf();
    }

    private static IReadOnlyList<(string Value, bool Numeric)> GetCells(PurchaseSheet row, string dateFormat, bool withValues)
    {
        var cells = new List<(string, bool)>
        {
            (row.BoxNo ?? string.Empty, false), (CompanyDateFormats.Format(row.SaleDate, dateFormat), false),
            (row.SaleType ?? string.Empty, false), (row.SaleCode ?? string.Empty, false), (row.BrokerName ?? string.Empty, false),
            (row.LotNo ?? string.Empty, false), (row.Invoice ?? string.Empty, false), (row.GardenMark ?? string.Empty, false),
            (row.Standard ?? string.Empty, false), (row.Grade ?? string.Empty, false)
        };
        if (withValues)
        {
            cells.AddRange([
                (Number(row.Packs), true), (Number(row.NetWeight), true), (Number(row.VirtualWeight), true),
                (Number(row.Rate), true), (Number(row.VirtualValue), true)
            ]);
        }
        return cells;
    }

    private static void AddSummaryRow(ColumnDescriptor summary, string label, string value) =>
        summary.Item().PaddingVertical(2).Row(row =>
        {
            row.ConstantItem(170).Text(label).Bold();
            row.ConstantItem(90).AlignLeft().Text(value);
        });

    private static string Number(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
    private static IContainer Cell(IContainer container) => container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Grey.Lighten3).Padding(4);
}
