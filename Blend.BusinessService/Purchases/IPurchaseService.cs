namespace Blend.BusinessService.Purchases;

public interface IPurchaseService
{
    IReadOnlyList<PurchaseSale> GetAllSales(string searchString, int companyId);
    IReadOnlyList<StraightLine> GetAddedStraightLines(int saleId);
    StraightLine? GetStraightLineByReservedNo(long straightLineId);
    IReadOnlyList<TeaCertificate> GetTeaCertificates();
    BusinessOperationResult CreateSale(PurchaseSale sale);
    BusinessOperationResult UpdateSale(PurchaseSale sale);
    BusinessOperationResult CloseSale(PurchaseSale sale);
    BusinessOperationResult OpenSale(PurchaseSale sale);
    BusinessOperationResult AddStraightLine(StraightLine line);
    BusinessOperationResult UpdateStraightLine(StraightLine line);
    BusinessOperationResult DeleteStraightLine(StraightLine line);
}
