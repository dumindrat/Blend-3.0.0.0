using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Purchase;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class StraightLineDeleteController : BaseApi
    {
         private IPurchaseService _purchaseSvc;
         public StraightLineDeleteController(IPurchaseService purchaseService)
        {
            _purchaseSvc = purchaseService;
        }

        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Put([FromBody]StraightLine starightLine)
        {
            if (ModelState.IsValid && SetUser(starightLine))
            {
                try
                {
                    _purchaseSvc.DeleteStraightLine(starightLine);
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
