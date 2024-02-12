using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.ItemSupplier;
using Blend.BuisnessService.Item;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Packing.ItemMaster
{
    public class SupplierController : BaseApi
    {
        private ISupplierService _supplierSvc;
        public SupplierController(ISupplierService supplierSvc)
        {
            _supplierSvc = supplierSvc;
        }

        [HttpGet]
        [ActionName("GetAllSupplier")]
        public IEnumerable<Supplier> GetAllSupplier(string id)
        {
            IEnumerable<Supplier> items = new List<Supplier>();

            if (ModelState.IsValid)
            {
                try
                {
                    items = _supplierSvc.GetAllSuppliers(id, GetCompanyId());
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return items;
        }

        [HttpGet]
        [ActionName("GetSupplier")]
        public Supplier GetSupplier(string id)
        {
            var supplier = new Supplier();

            if (ModelState.IsValid)
            {
                try
                {
                    supplier = _supplierSvc.GetSupplierById(Convert.ToInt16(id));
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return supplier;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]Supplier value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _supplierSvc.SaveSupplier(value);
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
        public HttpResponseMessage Put([FromBody]Supplier value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _supplierSvc.UpdateSupplier(value);
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
                _supplierSvc.DeleteSupplier(id);
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
