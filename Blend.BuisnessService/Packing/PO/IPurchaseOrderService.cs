using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessEntities.Item;
using Blend.BuisnessEntities.Email;

namespace Blend.BuisnessService.Packing.PO
{
    public interface IPurchaseOrderService
    {
        IEnumerable<PurchaseOrderDto> GetPOList(string id);
        PurchaseOrderDto GetPOById(int id);
        bool CreatePO(PurchaseOrderInfo po);
        bool UpdatePO(PurchaseOrderInfo po);
        bool DeletePO(int id);
        bool AddItem(PurchaseOrderItemDto item);
        bool RemoveItem(PurchaseOrderItemDto item);
        IEnumerable<PurchaseOrderItemDto> GetPOItemList(int id);
    }
}
