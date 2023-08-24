using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.AuctionBrokers;
using Blend.BuisnessService.Broker;

namespace Blend.WebApp.Controllers.Tea.Broker
{
    public class BrokerController : BaseApi
    {
        private IBrokerService _brokerService;

        public BrokerController(IBrokerService brokerService)
        {
            _brokerService = brokerService;
        }

        [HttpGet]
        [ActionName("GetBrokers")]
        public IEnumerable<TeaBroker> Get(string id)
        {
            //var broker = _brokerService.GetBrokerById(1);
            return _brokerService.GetAllBrokers(id, GetCompanyId());
            
        }

        //// GET api/broker/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/broker
        public HttpResponseMessage Post([FromBody]TeaBroker broker)
        {
            if (ModelState.IsValid && SetUser(broker))
            {
                try
                {
                    _brokerService.CreateBroker(broker);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/broker/5
        public HttpResponseMessage Put(int id, [FromBody]TeaBroker value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _brokerService.UpdateBroker(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);

        }

        // DELETE api/broker/5
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _brokerService.DeleteBroker(id);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
    }
}
