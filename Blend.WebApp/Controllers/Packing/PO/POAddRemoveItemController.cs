using Blend.BuisnessEntities.Packing.PO;
using Blend.BuisnessService.Packing.PO;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.PO
{
    public class POAddRemoveItemController : BaseApi
    {
        private IPurchaseOrderService _poSvc;
        public POAddRemoveItemController(IPurchaseOrderService poSvc)
        {
            _poSvc = poSvc;
        }
        // POST api/blend
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]PurchaseOrderItemDto poitem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(poitem);
                    _poSvc.AddItem(poitem);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/blendaddremove/5
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]PurchaseOrderItemDto item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(item);
                    _poSvc.RemoveItem(item);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }


    }
}
