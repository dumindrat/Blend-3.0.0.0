using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Blend.BlazorApp.Authentication;
using Blend.BusinessService.Reports.Purchase;
using Dapper;

namespace Blend.BlazorApp.Services;

public enum SalesSummaryOutput { Pdf, Excel }

public interface ISalesSummaryReportService
{
    byte[] Generate(int saleId, string? companyName, int companyId, string printedBy, SalesSummaryOutput output = SalesSummaryOutput.Pdf, string dateFormat = CompanyDateFormats.MonthFirst);
}

public sealed class SaleNotInCompanyException() : Exception("The selected sale is no longer available for the working company.");
public sealed class EmptySalesSummaryException() : Exception("No sales summary data was found for the selected sale.");

public sealed class SalesSummaryReportService(
    IPurchaseReportService purchaseReports,
    SalesSummaryPdfService pdf,
    IHostEnvironment environment,
    Func<IDbConnection> connectionFactory) : ISalesSummaryReportService
{
    public byte[] Generate(int saleId, string? companyName, int companyId, string printedBy, SalesSummaryOutput output = SalesSummaryOutput.Pdf, string dateFormat = CompanyDateFormats.MonthFirst)
    {
        if (output == SalesSummaryOutput.Pdf && !environment.IsDevelopment())
            throw new InvalidOperationException("The QuestPDF evaluation report is available only in Development.");
        if (!Enum.IsDefined(output)) throw new ArgumentOutOfRangeException(nameof(output));
        if (!CompanyDateFormats.IsSupported(dateFormat)) throw new ArgumentOutOfRangeException(nameof(dateFormat));
        if (saleId <= 0) throw new ArgumentOutOfRangeException(nameof(saleId));
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        ArgumentException.ThrowIfNullOrWhiteSpace(printedBy);

        using var connection = connectionFactory();
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var ownedSale = connection.QuerySingleOrDefault<int?>(
            "SELECT Id FROM purchase_header WITH (HOLDLOCK) WHERE Id = @SaleId AND CompanyId = @CompanyId",
            new { SaleId = saleId, CompanyId = companyId }, transaction);
        if (ownedSale is null) throw new SaleNotInCompanyException();

        var rows = purchaseReports.GetSaleSummary(saleId);
        if (rows.Count == 0) throw new EmptySalesSummaryException();
        if (rows.Any(row => row is null || string.IsNullOrWhiteSpace(row.SaleDate)))
            throw new ArgumentException("Every sales summary row must have a sale date.", nameof(rows));
        var saleDate = rows[0].SaleDate!;
        if (rows.Any(row => !string.Equals(row.SaleDate, saleDate, StringComparison.Ordinal)))
            throw new ArgumentException("Sales summary rows must have the same sale date.", nameof(rows));
        var printedAt = DateTimeOffset.UtcNow;
        var result = output switch
        {
            SalesSummaryOutput.Pdf => pdf.Generate(companyName, rows, printedBy, printedAt, dateFormat),
            SalesSummaryOutput.Excel => GenerateExcel(saleId, companyName, rows, printedBy, printedAt, dateFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(output))
        };
        transaction.Commit();
        return result;
    }

    private static byte[] GenerateExcel(int saleId, string? companyName, IReadOnlyList<SaleSummary> rows, string printedBy, DateTimeOffset printedAt, string dateFormat)
    {
        var totals = SalesSummaryPdfService.CalculateTotals(rows);
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", writer =>
            {
                writer.WriteStartElement("Types", "http://schemas.openxmlformats.org/package/2006/content-types");
                WriteElement(writer, "Default", "http://schemas.openxmlformats.org/package/2006/content-types", ("Extension", "rels"), ("ContentType", "application/vnd.openxmlformats-package.relationships+xml"));
                WriteElement(writer, "Default", "http://schemas.openxmlformats.org/package/2006/content-types", ("Extension", "xml"), ("ContentType", "application/xml"));
                WriteElement(writer, "Override", "http://schemas.openxmlformats.org/package/2006/content-types", ("PartName", "/xl/workbook.xml"), ("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"));
                WriteElement(writer, "Override", "http://schemas.openxmlformats.org/package/2006/content-types", ("PartName", "/xl/worksheets/sheet1.xml"), ("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"));
                WriteElement(writer, "Override", "http://schemas.openxmlformats.org/package/2006/content-types", ("PartName", "/xl/styles.xml"), ("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"));
                writer.WriteEndElement();
            });
            AddEntry(archive, "_rels/.rels", writer =>
            {
                writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
                WriteElement(writer, "Relationship", "http://schemas.openxmlformats.org/package/2006/relationships", ("Id", "rId1"), ("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"), ("Target", "xl/workbook.xml"));
                writer.WriteEndElement();
            });
            AddEntry(archive, "xl/workbook.xml", writer =>
            {
                writer.WriteStartElement("workbook", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                writer.WriteStartElement("sheets", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                WriteElement(writer, "sheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main", ("name", "Sales Summary"), ("sheetId", "1"), ("r:id", "rId1"));
                writer.WriteEndElement();
                writer.WriteEndElement();
            });
            AddEntry(archive, "xl/_rels/workbook.xml.rels", writer =>
            {
                writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
                WriteElement(writer, "Relationship", "http://schemas.openxmlformats.org/package/2006/relationships", ("Id", "rId1"), ("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"), ("Target", "worksheets/sheet1.xml"));
                WriteElement(writer, "Relationship", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", ("Id", "rId2"), ("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"), ("Target", "styles.xml"));
                writer.WriteEndElement();
            });
            AddEntry(archive, "xl/styles.xml", WriteStyles);
            AddEntry(archive, "xl/worksheets/sheet1.xml", writer => WriteWorksheet(writer, saleId, companyName, rows, totals, printedBy, printedAt, dateFormat));
        }
        return stream.ToArray();
    }

    private static void WriteWorksheet(XmlWriter writer, int saleId, string? companyName, IReadOnlyList<SaleSummary> rows, SummaryTotals totals, string printedBy, DateTimeOffset printedAt, string dateFormat)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("worksheet", ns);
        writer.WriteStartElement("cols", ns);
        WriteElement(writer, "col", ns, ("min", "1"), ("max", "1"), ("width", "32"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "2"), ("max", "3"), ("width", "12"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "4"), ("max", "7"), ("width", "16"), ("customWidth", "1"));
        writer.WriteEndElement();
        writer.WriteStartElement("sheetData", ns);
        var row = 1;
        WriteTextRow(writer, row++, ("A", $"Sales Summary - {companyName ?? string.Empty}", 1));
        WriteTextRow(writer, row++, ("A", "Sale date", 1), ("B", CompanyDateFormats.Format(rows[0].SaleDate, dateFormat), 0), ("D", "Sale number", 1), ("E", saleId.ToString(CultureInfo.InvariantCulture), 0));
        row++;
        WriteTextRow(writer, row++, ("A", "Broker", 1), ("B", "Type", 1), ("C", "Lots", 1), ("D", "Weight", 1), ("E", "Value", 1), ("F", "90%", 1), ("G", "10%", 1));
        foreach (var item in rows)
            WriteTextAndNumberRow(writer, row++, item.BrokerName ?? string.Empty, item.SaleType ?? string.Empty, item.NoOfLots, item.Weight, item.Value, item.NinetyPercVal, item.TenPercVal);
        WriteTextAndNumberRow(writer, row++, "Total", string.Empty, totals.Lots, totals.Weight, totals.Value, totals.NinetyPercent, totals.TenPercent, bold: true);
        row++;
        WriteTextRow(writer, row++, ("A", "Summary", 1));
        WriteSummaryRow(writer, row++, "No Of Lots:", totals.Lots.ToString(CultureInfo.InvariantCulture), true);
        WriteSummaryRow(writer, row++, "Total Sale Qty:", totals.Weight.ToString(CultureInfo.InvariantCulture), false);
        WriteSummaryRow(writer, row++, "Total Value:", totals.Value.ToString(CultureInfo.InvariantCulture), false);
        WriteSummaryRow(writer, row++, "Total 10% of Adv:", totals.TenPercent.ToString(CultureInfo.InvariantCulture), false);
        WriteSummaryRow(writer, row++, "Total 90% Bal:", totals.NinetyPercent.ToString(CultureInfo.InvariantCulture), false);
        WriteSummaryRow(writer, row++, "Sale Avg:", totals.Average?.ToString(CultureInfo.InvariantCulture) ?? "N/A", totals.Average is not null);
        row++;
        WriteTextRow(writer, row++, ("A", "Printed by", 1), ("B", printedBy, 0));
        WriteTextRow(writer, row, ("A", "Printed at (UTC)", 1), ("B", printedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), 0));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteTextAndNumberRow(XmlWriter writer, int row, string broker, string type, int lots, decimal weight, decimal value, decimal ninety, decimal ten, bool bold = false)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        WriteTextCell(writer, "A", row, broker, bold ? 1 : 0);
        WriteTextCell(writer, "B", row, type, bold ? 1 : 0);
        WriteNumberCell(writer, "C", row, lots.ToString(CultureInfo.InvariantCulture), 3);
        WriteNumberCell(writer, "D", row, weight.ToString(CultureInfo.InvariantCulture), 2);
        WriteNumberCell(writer, "E", row, value.ToString(CultureInfo.InvariantCulture), 2);
        WriteNumberCell(writer, "F", row, ninety.ToString(CultureInfo.InvariantCulture), 2);
        WriteNumberCell(writer, "G", row, ten.ToString(CultureInfo.InvariantCulture), 2);
        writer.WriteEndElement();
    }

    private static void WriteTextRow(XmlWriter writer, int row, params (string Column, string Value, int Style)[] cells)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        foreach (var cell in cells) WriteTextCell(writer, cell.Column, row, cell.Value, cell.Style);
        writer.WriteEndElement();
    }

    private static void WriteSummaryRow(XmlWriter writer, int row, string label, string value, bool integer)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        WriteTextCell(writer, "A", row, label, 1);
        if (integer) WriteNumberCell(writer, "B", row, value, 3);
        else if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) WriteNumberCell(writer, "B", row, value, 2);
        else WriteTextCell(writer, "B", row, value, 0);
        writer.WriteEndElement();
    }

    private static void WriteTextCell(XmlWriter writer, string column, int row, string value, int style)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("c", ns);
        writer.WriteAttributeString("r", column + row.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("t", "inlineStr");
        if (style != 0) writer.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));
        writer.WriteStartElement("is", ns);
        writer.WriteStartElement("t", ns);
        writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
        writer.WriteString(value);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNumberCell(XmlWriter writer, string column, int row, string value, int style)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("c", ns);
        writer.WriteAttributeString("r", column + row.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));
        writer.WriteElementString("v", ns, value);
        writer.WriteEndElement();
    }

    private static void WriteStyles(XmlWriter writer)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("styleSheet", ns);
        writer.WriteStartElement("fonts", ns); writer.WriteAttributeString("count", "2");
        WriteFont(writer, false); WriteFont(writer, true); writer.WriteEndElement();
        writer.WriteStartElement("fills", ns); writer.WriteAttributeString("count", "2");
        writer.WriteStartElement("fill", ns); writer.WriteStartElement("patternFill", ns); writer.WriteAttributeString("patternType", "none"); writer.WriteEndElement(); writer.WriteEndElement();
        writer.WriteStartElement("fill", ns); writer.WriteStartElement("patternFill", ns); writer.WriteAttributeString("patternType", "gray125"); writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndElement();
        writer.WriteStartElement("borders", ns); writer.WriteAttributeString("count", "1"); writer.WriteStartElement("border", ns);
        foreach (var side in new[] { "left", "right", "top", "bottom", "diagonal" }) writer.WriteElementString(side, ns, "");
        writer.WriteEndElement(); writer.WriteEndElement();
        writer.WriteStartElement("cellStyleXfs", ns); writer.WriteAttributeString("count", "1"); WriteElement(writer, "xf", ns, ("numFmtId", "0"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0")); writer.WriteEndElement();
        writer.WriteStartElement("cellXfs", ns); writer.WriteAttributeString("count", "4");
        WriteElement(writer, "xf", ns, ("numFmtId", "0"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0"), ("xfId", "0"));
        WriteElement(writer, "xf", ns, ("numFmtId", "0"), ("fontId", "1"), ("fillId", "0"), ("borderId", "0"), ("applyFont", "1"), ("xfId", "0"));
        WriteElement(writer, "xf", ns, ("numFmtId", "4"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0"), ("applyNumberFormat", "1"), ("xfId", "0"));
        WriteElement(writer, "xf", ns, ("numFmtId", "1"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0"), ("applyNumberFormat", "1"), ("xfId", "0"));
        writer.WriteEndElement();
        writer.WriteStartElement("cellStyles", ns); writer.WriteAttributeString("count", "1"); WriteElement(writer, "cellStyle", ns, ("name", "Normal"), ("xfId", "0"), ("builtinId", "0")); writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFont(XmlWriter writer, bool bold)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("font", ns);
        if (bold) writer.WriteElementString("b", ns, "");
        WriteElement(writer, "name", ns, ("val", "Calibri"));
        WriteElement(writer, "sz", ns, ("val", "11"));
        writer.WriteEndElement();
    }

    private static void AddEntry(ZipArchive archive, string name, Action<XmlWriter> write)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new System.Text.UTF8Encoding(false), Indent = false, CloseOutput = false });
        writer.WriteStartDocument();
        write(writer);
        writer.WriteEndDocument();
    }

    private static void WriteElement(XmlWriter writer, string name, string ns, params (string Name, string Value)[] attributes)
    {
        writer.WriteStartElement(name, ns);
        foreach (var attribute in attributes)
        {
            if (attribute.Name == "r:id") writer.WriteAttributeString("r", "id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", attribute.Value);
            else writer.WriteAttributeString(attribute.Name, attribute.Value);
        }
        writer.WriteEndElement();
    }
}
