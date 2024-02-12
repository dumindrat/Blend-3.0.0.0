using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Item;

namespace Blend.BuisnessService.Item
{
    public class ItemService : BaseManagement<ItemMaster>, IItemService
    {
        public IEnumerable<ItemMaster> GetAllItems(string searchStr, int companyId)
        {
            var searchBy = searchStr == "all" ? "" : searchStr;
            var mapper = new DataMapper<ItemMaster>("GetItems", new Hashtable() { { "ItemName", searchBy }, { "CompanyId", companyId } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool SaveItem(BuisnessEntities.Item.ItemMaster item)
        {
            dataOperation.CommandText("pack_ItemMasterInsert");
            dataOperation.AddParam("@ItemCode", item.ItemCode);
            dataOperation.AddParam("@ItemName", item.ItemName);
            dataOperation.AddParam("@User", item.User);
            dataOperation.AddParam("@UomId", item.UomId);
            dataOperation.AddParam("@ShortDescription", item.ShortDescription);
            dataOperation.AddParam("@CompanyId", item.CompanyId);
            dataOperation.DoExecute();
            return true;
        }

        public bool UpdateItem(BuisnessEntities.Item.ItemMaster item)
        {
            dataOperation.CommandText("pack_ItemMasterUpdate");
            dataOperation.AddParam("@Id", item.Id);
            dataOperation.AddParam("@ItemCode", item.ItemCode);
            dataOperation.AddParam("@ItemName", item.ItemName);
            dataOperation.AddParam("@UomId", item.UomId);
            dataOperation.AddParam("@ShortDescription", item.ShortDescription);
            dataOperation.AddParam("@User", item.User);
            dataOperation.AddParam("@RowVersion", item.RowVersion);
            dataOperation.DoExecute();
            return true;
        }

        public bool DeleteItem(BuisnessEntities.Item.ItemMaster item)
        {
            dataOperation.CommandText("pack_ItemMasterDelete");
            dataOperation.AddParam("@Id", item.Id);
            dataOperation.DoExecute();
            return true;
        }

        public bool AddItemProperty(BuisnessEntities.Item.ItemProperty itemProperty)
        {
            throw new NotImplementedException();
        }

        public bool UpdateItemProperty(BuisnessEntities.Item.ItemProperty itemProperty)
        {
            throw new NotImplementedException();
        }

        public bool DeleteItemProperty(BuisnessEntities.Item.ItemProperty itemProperty)
        {
            throw new NotImplementedException();
        }
    }
}
