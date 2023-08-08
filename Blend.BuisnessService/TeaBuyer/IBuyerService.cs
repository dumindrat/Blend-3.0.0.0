using Blend.BuisnessEntities.TeaBuyer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TeaBuyer
{
    public interface IBuyerService
    {
        Buyer GetBuyerById(int buyerId);
        IList<Buyer> GetAllBuyers(string serchStr);
        bool CreateBuyer(Buyer buyerEntity);
        bool UpdateBuyer(int buyerId, Buyer buyerEntity);
        bool DeleteBuyer(int buyerId);
    }
}
