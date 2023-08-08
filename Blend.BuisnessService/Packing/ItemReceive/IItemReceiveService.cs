using Blend.BuisnessEntities.Packing.ItemReceive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.ItemReceive
{
    public interface IItemReceiveService
    {
        IEnumerable<ItemReceiveNoteDto> GetItemReceiveNoteList(string id);
        bool Create(ItemReceiveNoteDto receivenote);
        bool AddItemQty(UpdateReceivedItemDto receivenoteitem);
        bool UpdateItemQty(UpdateReceivedItemDto receivenoteitem);
        IEnumerable<AvailablePoDto> GetPoList();
        IEnumerable<ReceiveNoteItems> GetReceiveNoteItems(int id);
        ItemReceiveNoteDto GetReceiveNoteById(int id);

    }
}
