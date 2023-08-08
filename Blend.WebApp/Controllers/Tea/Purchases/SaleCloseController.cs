using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Purchase;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class SaleCloseController : BaseApi
    {
       private IPurchaseService _purchaseSvc;

       public SaleCloseController(IPurchaseService purchaseService)
        {
            _purchaseSvc = purchaseService;
        }

       public HttpResponseMessage Post([FromBody]Sale sale)
       {

           if (ModelState.IsValid && SetUser(sale))
           {
               try
               {
                   if (sale.IsClosed)
                   {
                       _purchaseSvc.CloseSale(sale);
                   }
                   else
                   {
                       _purchaseSvc.OpenSale(sale);
                   }
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
