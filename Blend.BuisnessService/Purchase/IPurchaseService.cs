using Blend.BuisnessEntities.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Purchase
{
    public interface IPurchaseService
    {
        IEnumerable<Sale> GetAllSales(string searchString);
        StraightLine GetStraightLineByReservedNo(long straighlineId);
        bool CreateSale(Sale SaleEntity);
        bool UpdateSale(Sale SaleEntity);

        bool CloseSale(Sale SaleEntity);
        bool OpenSale(Sale SaleEntity);

        bool AddStraightLine(StraightLine StrightLineEntity);
        bool UpdateStraightLine(StraightLine StrightLineEntity);
        bool DeleteStraightLine(StraightLine StrightLineEntity);

        IEnumerable<StraightLine> GetAddedStraighLIne(int saleId);
        IEnumerable<TeaCertificates> GetTeaCertificates();

    }
}
