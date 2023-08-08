using Blend.BuisnessEntities.Packing.ItemIssue;
using Blend.BuisnessEntities.Packing.ItemReceive;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessService.Packing.ItemIssue;
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

namespace Blend.WebApp.Controllers.Packing.ItemIssue
{
    public class ItemIssueController : BaseApi
    {
        public IItemIssueService _itemIssueSvc;
        public ItemIssueController(IItemIssueService itemIssueSvc)
        {
            _itemIssueSvc = itemIssueSvc;
        }

        [HttpGet]
        [ActionName("GetAllIssueNotes")]
        public IEnumerable<ItemIssueNoteDto> GetAllIssueNotes(string id)
        {
            IEnumerable<ItemIssueNoteDto> pos = new List<ItemIssueNoteDto>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemIssueSvc.GetItemIssueNoteList(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetAllStockItems")]
        public IEnumerable<StockItem> GetAllStockItems(string id)
        {
            IEnumerable<StockItem> pos = new List<StockItem>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemIssueSvc.GetStockItemList(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [HttpGet]
        [ActionName("GetIssueNoteItemsById")]
        public IEnumerable<IssueNoteItem> GetIssueNoteItemsById(string id)
        {
            IEnumerable<IssueNoteItem> pos = new List<IssueNoteItem>();

            if (ModelState.IsValid)
            {
                try
                {
                    pos = _itemIssueSvc.GetIssueNoteItemById(Convert.ToInt16(id));
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return pos;

        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]ItemIssueNoteDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemIssueSvc.Create(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (SqlException ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Server Error in creating this transaction.");

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
        public HttpResponseMessage Put([System.Web.Http.FromBody]ItemIssueNoteDto item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(item);
                    _itemIssueSvc.Update(item);
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
                    _itemIssueSvc.Delete(id);
                    return new HttpResponseMessage(HttpStatusCode.OK);
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
