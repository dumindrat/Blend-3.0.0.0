using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessService.BlendClose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Tea.BlendCloseReversal
{
    public class BlendCloseReversalController : BaseApi
    {
        private IBlendCloseService _blendSvc;
        public BlendCloseReversalController(IBlendCloseService blendSvc)
        {
            _blendSvc = blendSvc;
        }

        [HttpGet]
        [ActionName("GetBlends")]
        public IEnumerable<BlendCloseDto> Get(string id)
        {
            IEnumerable<BlendCloseDto> blends = new List<BlendCloseDto>();
            try
            {
                blends = _blendSvc.GetBlendCloseList(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return blends;
            }
            return blends;
        }


        public HttpResponseMessage Post([FromBody]BlendCloseReversDto blend)
        {

            if (ModelState.IsValid && SetUser(blend))
            {
                try
                {
                    _blendSvc.BlendCloseCancelRequest(blend);
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
