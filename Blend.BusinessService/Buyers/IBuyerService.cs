namespace Blend.BusinessService.Buyers;

public interface IBuyerService
{
    IReadOnlyList<Buyer> GetAllBuyers(string searchText, int companyId);
    BusinessOperationResult CreateBuyer(Buyer buyer);
    BusinessOperationResult UpdateBuyer(Buyer buyer);
    BusinessOperationResult DeleteBuyer(int buyerId);
}
