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
    public class CancellationApproveController : BaseApi
    {
        private IAdjustmentService _adjustLineService;
        public CancellationApproveController(IAdjustmentService adjustLineService)
        {
            _adjustLineService = adjustLineService;
        }

        [HttpGet]
        [ActionName("GetAdjustedLine")]
        public IEnumerable<AdjutLineCancelRequest> GetAdjustedLine(string id)
        {
            IEnumerable<AdjutLineCancelRequest> data = new List<AdjutLineCancelRequest>();
            try
            {
                data = _adjustLineService.GetAdjusteLineCancelRequests(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return data;
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]AdjutLineCancelRequest value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _adjustLineService.ApproveCancelRequest(value);
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
