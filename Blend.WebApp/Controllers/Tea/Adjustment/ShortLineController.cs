using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessService.Adjustent;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.StockOut.Adjustment
{
    public class ShortLineController : BaseApi
    {
        private IAdjustmentService _adjustmentService;
        public ShortLineController(IAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]AdjutLineDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _adjustmentService.AddToShort(value);
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
