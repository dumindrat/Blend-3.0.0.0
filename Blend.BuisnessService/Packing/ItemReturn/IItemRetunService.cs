using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.ItemReturn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.ItemReturn
{
    public interface IItemRetunService
    {
        IEnumerable<ItemReturnNoteDto> GetItemReturNoteList(string id,int companyId);
        ItemReturnNoteDto GetReturNoteById(int id);
        IEnumerable<ItemReceiveNoteDto> GetReceiveNoteList();
        IEnumerable<ReturnNoteItems> GetReturnNoteItemById(int id);
        IEnumerable<ReceiveNoteStockItems> GetReceiveNoteItemsforReturn(int id);
        bool Create(ItemReturnNoteDto obj);
        bool Update(ItemReturnNoteDto obj);
        bool Delete(int id);
        bool AddItem(ReturnNoteItems item);
        bool RemoveItem(ReturnNoteItems item);

    }
}
