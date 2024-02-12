using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessService.BlendCreation;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.BlendCreation
{
    public class BlendEnableTeaBoardController : BaseApi
    {
        private IBlendCreationService _blendSvc;

        public BlendEnableTeaBoardController(IBlendCreationService blendSvc)
        {
            _blendSvc = blendSvc;
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Post([FromBody]string blend)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var blendInfo = new BlendInfo() { BlendId = Convert.ToInt16(blend) };
                    SetUser(blendInfo);
                    _blendSvc.EnableTeaBoard(blendInfo);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error");
        }

        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Put([FromBody]string blend)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var blendInfo = new BlendInfo() { BlendId = Convert.ToInt16(blend) };
                    SetUser(blendInfo);
                    _blendSvc.DisableTeaBoard(blendInfo);
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
