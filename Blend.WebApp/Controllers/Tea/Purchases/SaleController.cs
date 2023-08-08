using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Purchase;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class SaleController : BaseApi
    {
        private IPurchaseService _purchaseSvc;

        public SaleController(IPurchaseService purchaseService)
        {
            _purchaseSvc = purchaseService;
        }
        // GET api/sale
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public string Get(int id)
        {
            return "value";
        }

        public HttpResponseMessage Post([FromBody]Sale sale)
        {

            if (ModelState.IsValid && SetUser(sale))
            {
                try
                {
                    _purchaseSvc.CreateSale(sale);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        public HttpResponseMessage Put([FromBody]Sale sale)
        {

            if (ModelState.IsValid && SetUser(sale))
            {
                try
                {
                    _purchaseSvc.UpdateSale(sale);

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
