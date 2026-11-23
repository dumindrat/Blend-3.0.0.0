using System.Data;
using Blend.BlazorApp.Authentication;
using Blend.BusinessService.Reports.Purchase;
using Dapper;

namespace Blend.BlazorApp.Services;

public enum PurchaseSheetReportOutput { Pdf, Excel }

public interface IPurchaseSheetReportService
{
    byte[] Generate(int saleId, string? companyName, int companyId, string printedBy, PurchaseSheetReportOutput output, string dateFormat, bool withValues);
}

public sealed class EmptyPurchaseSheetException() : Exception("No purchase sheet data was found for the selected sale.");

public sealed class PurchaseSheetReportService(
    IPurchaseReportService purchaseReports,
    PurchaseSheetPdfService pdf,
    PurchaseSheetExcelService excel,
    IHostEnvironment environment,
    Func<IDbConnection> connectionFactory) : IPurchaseSheetReportService
{
    public byte[] Generate(int saleId, string? companyName, int companyId, string printedBy, PurchaseSheetReportOutput output, string dateFormat, bool withValues)
    {
        if (output == PurchaseSheetReportOutput.Pdf && !environment.IsDevelopment())
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

        var rows = purchaseReports.GetPurchaseSheet(saleId);
        if (rows.Count == 0) throw new EmptyPurchaseSheetException();
        var printedAt = DateTimeOffset.UtcNow;
        var document = output switch
        {
            PurchaseSheetReportOutput.Pdf => pdf.Generate(saleId, companyName, rows, printedBy, printedAt, dateFormat, withValues),
            PurchaseSheetReportOutput.Excel => excel.Generate(saleId, companyName, rows, printedBy, printedAt, dateFormat, withValues),
            _ => throw new ArgumentOutOfRangeException(nameof(output))
        };
        transaction.Commit();
        return document;
    }
}
