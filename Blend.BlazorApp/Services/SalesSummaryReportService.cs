using System.Data;
using Blend.BusinessService.Reports.Purchase;
using Dapper;

namespace Blend.BlazorApp.Services;

public interface ISalesSummaryReportService
{
    byte[] Generate(int saleId, string? companyName, int companyId, string printedBy);
}

public sealed class SaleNotInCompanyException() : Exception("The selected sale is no longer available for the working company.");
public sealed class EmptySalesSummaryException() : Exception("No sales summary data was found for the selected sale.");

public sealed class SalesSummaryReportService(
    IPurchaseReportService purchaseReports,
    SalesSummaryPdfService pdf,
    IHostEnvironment environment,
    Func<IDbConnection> connectionFactory) : ISalesSummaryReportService
{
    public byte[] Generate(int saleId, string? companyName, int companyId, string printedBy)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("The QuestPDF evaluation report is available only in Development.");
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
        var result = pdf.Generate(companyName, rows, printedBy, DateTimeOffset.UtcNow);
        transaction.Commit();
        return result;
    }
}
