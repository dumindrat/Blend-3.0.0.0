using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Item;
using Blend.BuisnessEntities.ItemSupplier;

namespace Blend.BuisnessService.Item
{
    public interface ISupplierService
    {
        IEnumerable<Supplier> GetAllSuppliers(string searchStr);
        Supplier GetSupplierById(int id);
        bool SaveSupplier(Supplier supplier);
        bool UpdateSupplier(Supplier supplier);
        bool DeleteSupplier(int id);

    }
}
