using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessService.TeaBuyer;
using Blend.Data;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.Buyer
{
    public class BuyerController : BaseApi
    {

        private IBuyerService _buyerService;
        public BuyerController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

        [HttpGet]
        [ActionName("GetAllBuyers")]
        public IEnumerable<Blend.BuisnessEntities.TeaBuyer.Buyer> GetAllBuyers(string id)
        {
            IList<Blend.BuisnessEntities.TeaBuyer.Buyer> buyers = new List<Blend.BuisnessEntities.TeaBuyer.Buyer>();
            if (ModelState.IsValid)
            {
                try
                {
                    buyers = _buyerService.GetAllBuyers(id);
                }
                catch (Exception ex)
                {
                    // logger.Error(ex.Message);
                }
            }
            return buyers;
        }

        // GET api/buyer/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/buyer
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]Blend.BuisnessEntities.TeaBuyer.Buyer value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _buyerService.CreateBuyer(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/buyer/5
        public HttpResponseMessage Put(int id, [FromBody]Blend.BuisnessEntities.TeaBuyer.Buyer value)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _buyerService.UpdateBuyer(value.BuyerId, value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    var response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    return response;
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // DELETE api/buyer/5
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _buyerService.DeleteBuyer(id);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
