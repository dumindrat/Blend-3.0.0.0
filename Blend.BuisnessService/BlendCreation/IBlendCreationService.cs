using Blend.BuisnessEntities.BlendCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.BlendCreation
{
    public interface IBlendCreationService
    {
        IList<BlendInfo> GetBlendList(string serchBlendNo, int companyId);
        BlendDetailDto GetBlendById(long id);
        BlendHeader GetBlendInfoById(long id);
        IList<BlendItemAllocation> GetBlendAllocationsById(long id, string itemType);
        IList<BlendItemAllocation> GetBlendAllocationsByBoxNo(string boxNo);
        bool CreateBlend(BlendInfo obj);
        bool UpdateBlend(BlendInfo obj);
        bool DeleteBlend(long blendId);
        bool AddItem(BlendItem obj);
        bool RemoveItem(BlendItem obj);
        IList<StraightLineStockItem> GetStraightLIneStock(string serchString, int companyId);
        IList<BlendStockItem> GetBlendStock(string serchString, int companyId);
        StraighLineStockDto GetStraightLIneStockByBoxNo(string boxNo);
        IList<BlendQuickSearchItem> GetBlendQuickSearchItem(string boxno);
        BlendStockItem GetBlendStockItemByStockId(long id);
        bool EnableTeaBoard(BlendInfo obj);
        bool DisableTeaBoard(BlendInfo obj);
    }
}
