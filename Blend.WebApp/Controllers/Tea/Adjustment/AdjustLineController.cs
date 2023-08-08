using Blend.BuisnessEntities.Adjustment;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessService.Adjustent;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Blend.WebApp.Controllers.StockOut.Adjustment
{
    public class AdjustLineController : BaseApi
    {
        private IAdjustmentService _adjustLineService;

        public AdjustLineController(IAdjustmentService adjustLineService)
        {
            _adjustLineService = adjustLineService;
        }

        [HttpGet]
        [ActionName("GetStraightLines")]
        public IEnumerable<StraightLineStockItem> GetStraightLines(string id)
        {
            IEnumerable<StraightLineStockItem> data = new List<StraightLineStockItem>();
            try
            {
                id = id.Replace("~", "/");
                data = _adjustLineService.GetStraightLines(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return data;
        }

        [HttpGet]
        [ActionName("GetAdjustedLine")]
        public IEnumerable<AdjutedLineDto> GetAdjustedLine(int id)
        {
            IEnumerable<AdjutedLineDto> data = new List<AdjutedLineDto>();
            try
            {
                data = _adjustLineService.GetAdjustedLineById(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return data;
        }

        [HttpGet]
        [ActionName("GetAdjustedLineByBrokerAndType")]
        public IEnumerable<PrintNoteDto> GetAdjustedLineByBrokerAndType(int brokerId, int type, int status)
        {
            IEnumerable<PrintNoteDto> data = new List<PrintNoteDto>();
            try
            {
                data = _adjustLineService.GetAdjustedLinesByBrokerAndType(brokerId, type, status);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return data;
        }

        [HttpGet]
        [ActionName("GetAdjustPaymentSettle")]
        public HttpResponseMessage GetAdjustPaymentSettle(int id)
        {
            try
            {
                var res = _adjustLineService.SettlePayment(id, GetUser());
                return new HttpResponseMessage(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {

                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]AdjutLineCancelRequest value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _adjustLineService.CancelRequest(value);
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
