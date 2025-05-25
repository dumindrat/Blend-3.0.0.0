using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.ItemReceive
{
    public class ItemReceiveController : BaseApi
    {
        public IItemReceiveService _itemReceiveSvc;
        public ItemReceiveController(IItemReceiveService itemReceiveSvc)
        {
            _itemReceiveSvc = itemReceiveSvc;
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
                    pos = _itemReceiveSvc.GetItemReceiveNoteList(id, GetCompanyId());
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetReceiveNotesItems")]
        public IEnumerable<ReceiveNoteItems> GetReceiveNotesItems(string id)
        {
            IEnumerable<ReceiveNoteItems> pos = new List<ReceiveNoteItems>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReceiveSvc.GetReceiveNoteItems(Convert.ToInt16(id));
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetPo")]
        public IEnumerable<AvailablePoDto> GetPo(string id)
        {
            IEnumerable<AvailablePoDto> pos = new List<AvailablePoDto>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemReceiveSvc.GetPoList(GetCompanyId());
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]ItemReceiveNoteDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemReceiveSvc.Create(value);
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
        public HttpResponseMessage Put([System.Web.Http.FromBody]UpdateReceivedItemDto item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(item);
                    _itemReceiveSvc.UpdateItemQty(item);
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
