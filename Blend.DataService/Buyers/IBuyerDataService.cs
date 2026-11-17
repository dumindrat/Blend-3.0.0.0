using System.Collections.Generic;

namespace Blend.DataService.Buyers
{
    public interface IBuyerDataService
    {
        IReadOnlyList<BuyerRecord> GetAllBuyers(string searchText, int companyId);
        bool CreateBuyer(BuyerRecord buyer);
        bool UpdateBuyer(BuyerRecord buyer);
        bool DeleteBuyer(int buyerId);
    }
}
