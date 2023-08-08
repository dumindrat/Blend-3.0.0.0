using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessService.BlendRateConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.BlendGainLossConvert
{
    public class BlendGainLossConvertController : BaseApi
    {
        private IBlendGainLossConvertService _rateConvertService;
        public BlendGainLossConvertController(IBlendGainLossConvertService rateConvertService)
        {
            _rateConvertService = rateConvertService;
        }

        [HttpGet]
        [ActionName("GetRate")]
        public decimal GetRate(string id)
        {
            decimal rate = 0;
            try
            {
                rate = _rateConvertService.GetRate(Convert.ToInt16(id));
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
            }
            return rate;
        }

        public HttpResponseMessage Post([FromBody]BlendGainLossRateConvert obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    if (obj.BG > 0)
                        _rateConvertService.ConvertGain(obj);
                    if (obj.BL > 0)
                        _rateConvertService.ConvertLoss(obj);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
