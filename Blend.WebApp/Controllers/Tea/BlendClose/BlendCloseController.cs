using Blend.BuisnessEntities.BlendClose;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessService.BlendClose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessEntities.Email;
using Blend.BuisnessEntities.Shipments;
using Blend.BuisnessService.Shipments;

namespace Blend.WebApp.Controllers.StockOut.BlendClose
{
    public class BlendCloseController : BaseApi
    {
        private IBlendCloseService _blendSvc;
        private IEmailService _emailService;
        private ITeaShipmentService _teaShipmentService;
        public BlendCloseController(IBlendCloseService blendSvc, IEmailService emailService, ITeaShipmentService teaShipmentService)
        {
            _blendSvc = blendSvc;
            _emailService = emailService;
            _teaShipmentService = teaShipmentService;
        }


        [HttpGet]
        [ActionName("GetBlends")]
        public IEnumerable<BlendInfo> Get(string id)
        {
            IEnumerable<BlendInfo> blends = new List<BlendInfo>();
            try
            {
                id = id.Replace("~", "/");
                blends = _blendSvc.GetBlendList(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return blends;
            }
            return blends;
        }

        [HttpGet]
        [ActionName("GetBlendCloseInfo")]
        public BlendCloseDto GetBlendCloseInfo(string id)
        {
            var blendCloseInfo = new BlendCloseDto();
            try
            {
                blendCloseInfo = _blendSvc.GetBlendCloseInfoByBlendid(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return blendCloseInfo;
            }
            return blendCloseInfo;
        }

        [HttpGet]
        [ActionName("GetClosedBlends")]
        public IEnumerable<ClosedBlend> GetClosedBlends(string id)
        {
            IEnumerable<ClosedBlend> blends = new List<ClosedBlend>();
            if (ModelState.IsValid)
            {
                try
                {
                    id = id.Replace("~", "/");
                    blends = _teaShipmentService.GetClosedBlends(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return blends;
        }

        // POST api/blendclose
        public HttpResponseMessage Post([FromBody]BlendCloseDto blend)
        {
            if (ModelState.IsValid && SetUser(blend))
            {
                try
                {
                    _blendSvc.BlendClose(blend);
                   // _emailService.SendMail(new EmailDto { Message = blend.BlendNo });
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        // PUT api/blendclose/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/blendclose/5
        public void Delete(int id)
        {
        }

        
    }
}
