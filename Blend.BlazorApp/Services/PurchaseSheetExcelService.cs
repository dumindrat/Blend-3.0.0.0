using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Blend.BlazorApp.Authentication;
using Blend.BusinessService.Reports.Purchase;

namespace Blend.BlazorApp.Services;

public sealed class PurchaseSheetExcelService
{
    public byte[] Generate(int saleId, string? companyName, IReadOnlyList<PurchaseSheet> rows, string printedBy, DateTimeOffset printedAt, string dateFormat, bool withValues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(printedBy);
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0) throw new ArgumentException("The purchase sheet contains no data.", nameof(rows));
        if (!CompanyDateFormats.IsSupported(dateFormat)) throw new ArgumentOutOfRangeException(nameof(dateFormat));

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
                WriteElement(writer, "sheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main", ("name", "Purchase Sheet"), ("sheetId", "1"), ("r:id", "rId1"));
                writer.WriteEndElement();
                writer.WriteEndElement();
            });
            AddEntry(archive, "xl/_rels/workbook.xml.rels", writer =>
            {
                writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
                WriteElement(writer, "Relationship", "http://schemas.openxmlformats.org/package/2006/relationships", ("Id", "rId1"), ("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"), ("Target", "worksheets/sheet1.xml"));
                WriteElement(writer, "Relationship", "http://schemas.openxmlformats.org/package/2006/relationships", ("Id", "rId2"), ("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"), ("Target", "styles.xml"));
                writer.WriteEndElement();
            });
            AddEntry(archive, "xl/styles.xml", WriteStyles);
            AddEntry(archive, "xl/worksheets/sheet1.xml", writer => WriteWorksheet(writer, saleId, companyName, rows, printedBy, printedAt, dateFormat, withValues));
        }
        return stream.ToArray();
    }

    private static void WriteWorksheet(XmlWriter writer, int saleId, string? companyName, IReadOnlyList<PurchaseSheet> rows, string printedBy, DateTimeOffset printedAt, string dateFormat, bool withValues)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var headers = withValues
            ? new[] { "No", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade", "Packs", "Net Weight", "Weight", "Rate", "Value" }
            : new[] { "No", "Box", "Sale Date", "Type", "Code", "Broker", "Lot", "Invoice", "Garden Mark", "Standard", "Grade" };
        writer.WriteStartElement("worksheet", ns);
        writer.WriteStartElement("cols", ns);
        WriteElement(writer, "col", ns, ("min", "1"), ("max", "1"), ("width", "5"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "2"), ("max", "3"), ("width", "9"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "4"), ("max", "6"), ("width", "4"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "7"), ("max", "7"), ("width", "9"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "8"), ("max", "8"), ("width", "11"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "9"), ("max", "9"), ("width", "20"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "12"), ("max", "12"), ("width", "6"), ("customWidth", "1"));
        WriteElement(writer, "col", ns, ("min", "13"), ("max", "13"), ("width", "9"), ("customWidth", "1"));
        writer.WriteEndElement();
        writer.WriteStartElement("sheetData", ns);
        var row = 1;
        WriteTextRow(writer, row++, ("A", $"Purchase Sheet - {companyName ?? string.Empty}", 1));
        WriteTextRow(writer, row++, ("A", "Sale date", 1), ("B", CompanyDateFormats.Format(rows[0].SaleDate, dateFormat), 0));
        row++;
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < headers.Length; index++) WriteTextCell(writer, ExcelColumn(index), row, headers[index], 1);
        writer.WriteEndElement();
        row++;
        for (var itemIndex = 0; itemIndex < rows.Count; itemIndex++)
        {
            var item = rows[itemIndex];
            var values = new List<(string? Text, decimal? Number)>
            {
                ((itemIndex + 1).ToString(CultureInfo.InvariantCulture), null), (item.BoxNo, null),
                (CompanyDateFormats.Format(item.SaleDate, dateFormat), null), (item.SaleType, null),
                (item.SaleCode, null), (item.BrokerName, null), (item.LotNo, null), (item.Invoice, null),
                (item.GardenMark, null), (item.Standard, null), (item.Grade, null)
            };
            if (withValues)
            {
                values.AddRange([
                    (null, item.Packs), (null, item.NetWeight), (null, item.VirtualWeight),
                    (null, item.Rate), (null, item.VirtualValue)
                ]);
            }
            writer.WriteStartElement("row", ns);
            writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
            for (var index = 0; index < headers.Length; index++)
            {
                var value = values[index];
                if (value.Number is decimal number) WriteNumberCell(writer, ExcelColumn(index), row, number.ToString(CultureInfo.InvariantCulture), 2);
                else WriteTextCell(writer, ExcelColumn(index), row, value.Text ?? string.Empty, 0);
            }
            writer.WriteEndElement();
            row++;
        }
        if (withValues)
        {
            writer.WriteStartElement("row", ns);
            writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
            WriteTextCell(writer, "F", row, "Total", 1);
            WriteNumberCell(writer, "N", row, rows.Sum(item => item.VirtualWeight).ToString(CultureInfo.InvariantCulture), 2);
            WriteNumberCell(writer, "P", row, rows.Sum(item => item.VirtualValue).ToString(CultureInfo.InvariantCulture), 2);
            writer.WriteEndElement();
            row++;
            var totalLots = rows.Select(item => item.LotNo?.Trim()).Where(lot => !string.IsNullOrEmpty(lot)).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            WriteNumberSummaryRow(writer, row++, "No of Lots :", totalLots);
            WriteNumberSummaryRow(writer, row++, "Tot No of Chests :", rows.Sum(item => item.Packs));
            WriteNumberSummaryRow(writer, row++, "Total Purchase Qty :", rows.Sum(item => item.VirtualWeight));
            WriteNumberSummaryRow(writer, row++, "Total Purchase Value :", rows.Sum(item => item.VirtualValue));
        }
        row++;
        WriteTextRow(writer, row++, ("A", "Printed by", 1), ("B", printedBy, 0));
        WriteTextRow(writer, row, ("A", "Printed at (UTC)", 1), ("B", printedAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), 0));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string ExcelColumn(int index)
    {
        var name = string.Empty;
        for (var value = index + 1; value > 0; value = (value - 1) / 26) name = (char)('A' + (value - 1) % 26) + name;
        return name;
    }

    private static void WriteTextRow(XmlWriter writer, int row, params (string Column, string Value, int Style)[] cells)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        foreach (var cell in cells) WriteTextCell(writer, cell.Column, row, cell.Value, cell.Style);
        writer.WriteEndElement();
    }

    private static void WriteNumberSummaryRow(XmlWriter writer, int row, string label, decimal value)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("row", ns);
        writer.WriteAttributeString("r", row.ToString(CultureInfo.InvariantCulture));
        WriteTextCell(writer, "A", row, label, 1);
        WriteNumberCell(writer, "B", row, value.ToString(CultureInfo.InvariantCulture), 2);
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
        writer.WriteStartElement("cellXfs", ns); writer.WriteAttributeString("count", "3");
        WriteElement(writer, "xf", ns, ("numFmtId", "0"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0"), ("xfId", "0"));
        WriteElement(writer, "xf", ns, ("numFmtId", "0"), ("fontId", "1"), ("fillId", "0"), ("borderId", "0"), ("applyFont", "1"), ("xfId", "0"));
        WriteElement(writer, "xf", ns, ("numFmtId", "4"), ("fontId", "0"), ("fillId", "0"), ("borderId", "0"), ("applyNumberFormat", "1"), ("xfId", "0"));
        writer.WriteEndElement();
        writer.WriteStartElement("cellStyles", ns); writer.WriteAttributeString("count", "1"); WriteElement(writer, "cellStyle", ns, ("name", "Normal"), ("xfId", "0"), ("builtinId", "0")); writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFont(XmlWriter writer, bool bold)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        writer.WriteStartElement("font", ns);
        if (bold) writer.WriteElementString("b", ns, string.Empty);
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
