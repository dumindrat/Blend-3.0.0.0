using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace Blend.BusinessService.Purchases;

public static class PurchaseCsvReader
{
    public static IReadOnlyList<PurchaseCsvRow> Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var parser = new TextFieldParser(stream, System.Text.Encoding.UTF8, true, leaveOpen: true) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");
        if (parser.EndOfData) throw new InvalidDataException("The CSV file is empty.");
        try { parser.ReadFields(); }
        catch (MalformedLineException ex) { throw new InvalidDataException("The CSV header is malformed.", ex); }
        var rows = new List<PurchaseCsvRow>();
        while (!parser.EndOfData)
        {
            var number = parser.LineNumber;
            string[] fields;
            try { fields = parser.ReadFields() ?? throw new InvalidDataException($"Row {number} is empty."); }
            catch (MalformedLineException ex) { throw new InvalidDataException($"CSV row {number} is malformed.", ex); }
            if (fields.Length != 14) throw new InvalidDataException($"CSV row {number} must contain 14 columns.");
            if (rows.Count >= 5000) throw new InvalidDataException("A file may contain at most 5,000 rows.");
            if (!decimal.TryParse(fields[6], NumberStyles.Number, CultureInfo.InvariantCulture, out var packs) ||
                !decimal.TryParse(fields[7], NumberStyles.Number, CultureInfo.InvariantCulture, out var net) ||
                !decimal.TryParse(fields[9], NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) ||
                !decimal.TryParse(fields[12], NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                throw new InvalidDataException($"CSV row {number} contains an invalid number.");
            var lot = fields[1].Trim(); var mark = fields[3].Trim(); var invoice = fields[4].Trim(); var grade = fields[5].Trim();
            if (lot.Length is 0 or > 10 || invoice.Length is 0 or > 10 || mark.Length is 0 or > 45 || grade.Length is 0 or > 45 ||
                !FitsDecimal(packs) || !FitsDecimal(net) || !FitsDecimal(quantity) || !FitsDecimal(rate) || packs <= 0 || net <= 0 || quantity < 0 || rate < 0)
                throw new InvalidDataException($"CSV row {number} has invalid lot, mark, invoice, grade, or quantities.");
            decimal gross;
            decimal value;
            try { gross = packs * net; value = quantity * rate; }
            catch (OverflowException ex) { throw new InvalidDataException($"CSV row {number} has values that are too large.", ex); }
            var reduction = gross - quantity;
            if (reduction < 0 || !FitsDecimal(reduction) || !FitsDecimal(value))
                throw new InvalidDataException($"CSV row {number} has invalid weight or value.");
            rows.Add(new PurchaseCsvRow(number, lot, mark, invoice, grade, packs, net, reduction, quantity, rate));
        }
        if (rows.Count == 0) throw new InvalidDataException("The CSV file contains no purchase rows.");
        return rows;
    }

    private static bool FitsDecimal(decimal value) => value >= -9999999999999999.99m && value <= 9999999999999999.99m && decimal.Round(value, 2) == value;
}

public sealed record PurchaseCsvRow(long RowNumber, string Lot, string Mark, string Invoice, string Grade, decimal Packs, decimal Net, decimal Reduction, decimal Quantity, decimal Rate);
