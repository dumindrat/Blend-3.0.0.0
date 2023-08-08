using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessEntities.Packing.ItemReturn;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessService.Packing.ItemIssue;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.ItemReturn;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.ItemIssue
{
    public class ItemReturnController : BaseApi
    {
        public IItemRetunService _itemReturnSvc;
        public ItemReturnController(IItemRetunService itemReturnSvc)
        {
            _itemReturnSvc = itemReturnSvc;
        }

        [HttpGet]
        [ActionName("GetAllReturnNotes")]
        public IEnumerable<ItemReturnNoteDto> GetAllReturnNotes(string id)
        {
            IEnumerable<ItemReturnNoteDto> pos = new List<ItemReturnNoteDto>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReturnSvc.GetItemReturNoteList(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetAllReceiveNotes")]
        public IEnumerable<ItemReceiveNoteDto> GetAllReceiveNotes(string id)
        {
            IEnumerable<ItemReceiveNoteDto> pos = new List<ItemReceiveNoteDto>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReturnSvc.GetReceiveNoteList();
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }
        
        [HttpGet]
        [ActionName("GetReturnNoteItemsById")]
        public IEnumerable<ReturnNoteItems> GetReturnNoteItemsById(int id)
        {
            IEnumerable<ReturnNoteItems> pos = new List<ReturnNoteItems>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReturnSvc.GetReturnNoteItemById(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetReceivedItems")]
        public IEnumerable<ReceiveNoteStockItems> GetReceivedItems(int id)
        {
            IEnumerable<ReceiveNoteStockItems> pos = new List<ReceiveNoteStockItems>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReturnSvc.GetReceiveNoteItemsforReturn(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }


        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]ItemReturnNoteDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    SetUser(value);
                    _itemReturnSvc.Create(value);
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
        public HttpResponseMessage Put([System.Web.Http.FromBody]ItemReturnNoteDto item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(item);
                    _itemReturnSvc.Update(item);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // DELETE api/blend/5
        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _itemReturnSvc.Delete(id);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (SqlException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }
    }
}
