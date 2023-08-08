using Blend.BuisnessService.GardenMarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Tea.GardenMark
{
    public class GardenMarkController : BaseApi
    {
         private IGardenMarkService _gardenMarkService;

         public GardenMarkController(IGardenMarkService gardenMarkService)
        {
            _gardenMarkService = gardenMarkService;
        }
        // GET api/document
         public IEnumerable<Blend.BuisnessEntities.Purchase.GardenMark> Get()
        {
            return _gardenMarkService.GetGardenMarksList();
        }

    }
}
