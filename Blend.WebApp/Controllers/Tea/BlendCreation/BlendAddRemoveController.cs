using Blend.BuisnessEntities.BlendCreation;
using Blend.BuisnessService.BlendCreation;
using Blend.WebApp.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.BlendCreation
{
    public class BlendAddRemoveController : BaseApi
    {
        private IBlendCreationService _blendSvc;

        public BlendAddRemoveController(IBlendCreationService blendSvc)
        {
            _blendSvc = blendSvc;
        }

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/blendaddremove/5
        public BlendDetailDto Get(int id)
        {
            var blend = new BlendDetailDto();
            try
            {
                blend = _blendSvc.GetBlendById(id);
            }
            catch (Exception ex)
            {

            }
            return blend;
        }

        // POST api/blend
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]List<BlendItem> blendItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var item in blendItems.Where(c => c.StockId > 0 && c.BlendQty > 0))
                    {
                        SetUser(item);
                        _blendSvc.AddItem(item);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/blendaddremove/5
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]BlendItem item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetUser(item);
                    _blendSvc.RemoveItem(item);
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
