using Blend.BuisnessEntities.Packing.ItemIssue;
using Blend.BuisnessService.Packing.ItemIssue;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.ItemIssue
{
    public class IssueNoteAddItemController : BaseApi
    {

        public IItemIssueService _itemIssueSvc;
        public IssueNoteAddItemController(IItemIssueService itemIssueSvc)
        {
            _itemIssueSvc = itemIssueSvc;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([System.Web.Http.FromBody]IssueNoteItem value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemIssueSvc.AddItem(value);
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
        public HttpResponseMessage Put([FromBody]IssueNoteItem value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemIssueSvc.RemoveItem(value);
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
