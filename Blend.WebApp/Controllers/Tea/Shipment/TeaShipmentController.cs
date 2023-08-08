using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Shipments;
using Blend.BuisnessService.Shipments;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.Shipment
{
    public class TeaShipmentController : BaseApi
    {
        private ITeaShipmentService _teaShipmentService;
        public TeaShipmentController(ITeaShipmentService teaShipmentService)
        {
            _teaShipmentService = teaShipmentService;
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

        // POST api/buyer
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]TeaShipmentDto value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _teaShipmentService.ShipTea(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    var response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
                    return response;
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

    }
}
