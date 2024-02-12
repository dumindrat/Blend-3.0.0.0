using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessService.Item;
using Blend.WebApp.App.ActionFilters;
using System.Data.SqlClient;

namespace Blend.WebApp.Controllers.Packing.ItemMaster
{
    public class ItemController : BaseApi
    {
        private IItemService _itemSvc;
        public ItemController(IItemService itemSvc)
        {
            _itemSvc = itemSvc;
        }

        [HttpGet]
        [ActionName("GetAllItems")]
        public IEnumerable<Blend.BuisnessEntities.Item.ItemMaster> GetAllItems(string id)
        {
            IEnumerable<Blend.BuisnessEntities.Item.ItemMaster> items = new List<Blend.BuisnessEntities.Item.ItemMaster>();

            if (ModelState.IsValid)
            {
                try
                {
                    items = _itemSvc.GetAllItems(id, GetCompanyId());
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return items;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]Blend.BuisnessEntities.Item.ItemMaster value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemSvc.SaveItem(value);
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
        public HttpResponseMessage Put([FromBody]Blend.BuisnessEntities.Item.ItemMaster value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _itemSvc.UpdateItem(value);
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
                _itemSvc.DeleteItem(new Blend.BuisnessEntities.Item.ItemMaster() { Id = id });
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (SqlException ex)
            {
                logger.LogWrite(ex.Message, "SQL Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error deletion.This Item may be allocated for transaction");

            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

    }
}
