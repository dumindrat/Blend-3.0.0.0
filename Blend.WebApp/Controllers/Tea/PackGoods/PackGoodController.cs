using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Blend.BuisnessEntities.PackGoods;
using Blend.BuisnessService.PackGoods;

namespace Blend.WebApp.Controllers.Tea.PackGoods
{
    public class PackGoodController : BaseApi
    {
        private IPackGoodService _packGoodService;

        public PackGoodController(IPackGoodService packGoodService)
        {
            _packGoodService = packGoodService;
        }

        public HttpResponseMessage Post([FromBody]PackGoodDto obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    _packGoodService.ConvertPackGoods(obj);
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
