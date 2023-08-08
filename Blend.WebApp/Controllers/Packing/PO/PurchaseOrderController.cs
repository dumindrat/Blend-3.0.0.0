using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Packing.PO;
using Blend.WebApp.App.ActionFilters;
using Blend.BuisnessEntities.Item;
using Blend.BuisnessService.Item;

namespace Blend.WebApp.Controllers.Packing.PO
{
    public class PurchaseOrderController : BaseApi
    {
        private IPurchaseOrderService _poSvc;
        private IItemService _itemSvc;
        public PurchaseOrderController(IPurchaseOrderService poSvc, IItemService itemService)
        {
            _poSvc = poSvc;
            _itemSvc = itemService;
        }

        [HttpGet]
        [ActionName("GetAllPO")]
        public IEnumerable<PurchaseOrderDto> GetAllPO(string id)
        {
            IEnumerable<PurchaseOrderDto> pos = new List<PurchaseOrderDto>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _poSvc.GetPOList(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetPOItems")]
        public PODto GetPOItems(string id)
        {
            //IEnumerable<PurchaseOrderItemDto> pos = new List<PurchaseOrderItemDto>();
            var purchaseOrderDto = new PODto();
            if (ModelState.IsValid)
            {
                try
                {
                    var pos = _poSvc.GetPOItemList(Convert.ToInt16(id));
                    purchaseOrderDto.Items = pos;
                    purchaseOrderDto.Value = pos.Sum(c => c.Value);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return purchaseOrderDto;

        }

        [HttpGet]
        [ActionName("GetAllPOItems")]
        public IEnumerable<PoItem> GetAllPOItems(string id, string poid)
        {
            IEnumerable<Blend.BuisnessEntities.Item.ItemMaster> items = new List<Blend.BuisnessEntities.Item.ItemMaster>();
            var poItems = new List<PoItem>();

            if (ModelState.IsValid)
            {
                try
                {
                    items = _itemSvc.GetAllItems(id);
                    var addedItems = _poSvc.GetPOItemList(Convert.ToInt16(poid));
                    foreach (var item in items)
                    {
                        var isAdded = addedItems.Where(c => c.ItemId == item.Id).SingleOrDefault();
                        poItems.Add(new PoItem
                        {
                            AddedDate = item.AddedDate,
                            CloseStatus = item.CloseStatus,
                            Id = item.Id,
                            ItemCode = item.ItemCode,
                            ItemName = item.ItemName,
                            ModifiedDate = item.ModifiedDate,
                            ModifiedUser = item.ModifiedUser,
                            ShortDescription = item.ShortDescription,
                            UomId = item.UomId,
                            UomName = item.UomName,
                            Qty = isAdded == null ? 0 : isAdded.Qty,
                            Price = isAdded == null ? 0 : isAdded.Price,
                            Comment = isAdded == null ? "" : isAdded.Comment,
                            IsAdded = isAdded == null ? false : true

                        });
                    }

                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return poItems;

        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]PurchaseOrderInfo value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _poSvc.CreatePO(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]PurchaseOrderInfo value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _poSvc.UpdatePO(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");

                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _poSvc.DeletePO(id);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        
    }
}
