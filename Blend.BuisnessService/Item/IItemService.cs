using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Item;

namespace Blend.BuisnessService.Item
{
    public interface IItemService
    {
        IEnumerable<ItemMaster> GetAllItems(string searchStr);
        bool SaveItem(ItemMaster item);
        bool UpdateItem(ItemMaster item);
        bool DeleteItem(ItemMaster item);

        bool AddItemProperty(ItemProperty itemProperty);
        bool UpdateItemProperty(ItemProperty itemProperty);
        bool DeleteItemProperty(ItemProperty itemProperty);

    }
}
