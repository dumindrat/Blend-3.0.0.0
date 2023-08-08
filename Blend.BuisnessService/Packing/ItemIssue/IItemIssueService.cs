using Blend.BuisnessEntities.Packing.ItemIssue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Packing.ItemIssue
{
    public interface IItemIssueService
    {
        IEnumerable<ItemIssueNoteDto> GetItemIssueNoteList(string id);
        IEnumerable<StockItem> GetStockItemList(string id);
        IEnumerable<IssueNoteItem> GetIssueNoteItemById(int id);
        bool Create(ItemIssueNoteDto obj);
        bool Update(ItemIssueNoteDto obj);
        bool Delete(int id);
        bool AddItem(IssueNoteItem item);
        bool RemoveItem(IssueNoteItem item);
    }
}
