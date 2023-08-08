using Blend.BuisnessEntities.Packing.ItemReturn;
using Blend.BuisnessService.Packing.ItemReturn;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.ItemReturn
{
    public class ItemReturnAddItemController : BaseApi
    {
        public IItemRetunService _itemReturnSvc;
        public ItemReturnAddItemController(IItemRetunService itemReturnSvc)
        {
            _itemReturnSvc = itemReturnSvc;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]ReturnNoteItems value)
        {
            try
            {
                SetUser(value);
                _itemReturnSvc.AddItem(value);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([System.Web.Http.FromBody]ReturnNoteItems value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    SetUser(value);
                    _itemReturnSvc.RemoveItem(value);
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

    }
}
