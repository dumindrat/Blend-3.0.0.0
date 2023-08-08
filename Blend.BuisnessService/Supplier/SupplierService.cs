using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Item;
using Blend.BuisnessEntities.ItemSupplier;

namespace Blend.BuisnessService.Item
{
    public class SupplierService : BaseManagement<Supplier>, ISupplierService
    {
        public IEnumerable<Supplier> GetAllSuppliers(string searchStr)
        {
            var searchBy = searchStr == "all" ? "" : searchStr;
            var mapper = new DataMapper<Supplier>("GetSuppliers", new Hashtable() { { "SupplierName", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool SaveSupplier(Supplier supplier)
        {
            dataOperation.CommandText("pack_SupplierInsert");
            dataOperation.AddParam("@SupplierName", supplier.SupplierName);
            dataOperation.AddParam("@Address1", supplier.Address1);
            dataOperation.AddParam("@Address2", supplier.Address2);
            dataOperation.AddParam("@Tel", supplier.Tel);
            dataOperation.AddParam("@Email", supplier.Email);
            dataOperation.AddParam("@User", supplier.User);
            dataOperation.DoExecute();
            return true;
        }
         
        public bool UpdateSupplier(Supplier supplier)
        {
            dataOperation.CommandText("pack_SupplierUpdate");
            dataOperation.AddParam("@Id", supplier.Id);
            dataOperation.AddParam("@SupplierName", supplier.SupplierName);
            dataOperation.AddParam("@Address1", supplier.Address1);
            dataOperation.AddParam("@Address2", supplier.Address2);
            dataOperation.AddParam("@Tel", supplier.Tel);
            dataOperation.AddParam("@Email", supplier.Email);
            dataOperation.AddParam("@User", supplier.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool DeleteSupplier(int id)
        {
            dataOperation.CommandText("pack_SupplierDelete");
            dataOperation.AddParam("@Id", id);
            dataOperation.DoExecute();
            return true;
        }


        public Supplier GetSupplierById(int id)
        {
            var mapper = new DataMapper<Supplier>("GetSupplier", new Hashtable() { { "Id", id } });

            var result = mapper.Object();
            return result;
        }
    }
}
