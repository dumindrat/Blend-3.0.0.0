using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessEntities.Purchase;
using Blend.BuisnessService.Purchase;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class StraightLinePurchaseController : BaseApi
    {
        private IPurchaseService _purchaseSvc;
        public StraightLinePurchaseController(IPurchaseService purchaseService)
        {
            _purchaseSvc = purchaseService;
        }

        // GET api/blendreports
        public IEnumerable<TeaCertificates> Get()
        {
            return _purchaseSvc.GetTeaCertificates();
        }
        // GET api/straightlinepurchase
        [HttpGet]
        [ActionName("GetAllSales")]
        public IEnumerable<Sale> Get(string id)
        {
            IEnumerable<Sale> sales = new List<Sale>();
            try
            {
                sales = _purchaseSvc.GetAllSales(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return sales;
            }
            return sales;
        }

        [HttpGet]
        [ActionName("GetAddedStraghtLine")]
        public IEnumerable<StraightLine> GetAddedStraghtLine(int id)
        {
            IEnumerable<StraightLine> sales = new List<StraightLine>();
            try
            {
                sales = _purchaseSvc.GetAddedStraighLIne(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return sales;
            }
            return sales;
        }

        [HttpGet]
        [ActionName("GetStraightLineById")]
        public StraightLine GetStraightLineById(long id)
        {
            var straightLIne = new StraightLine();
            try
            {
                straightLIne = _purchaseSvc.GetStraightLineByReservedNo(id);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return straightLIne;
            }
            return straightLIne;
        }

        // POST api/straightlinepurchase
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Post([FromBody]StraightLine starightLine)
        {
            if (ModelState.IsValid && SetUser(starightLine))
            {
                try
                {
                    _purchaseSvc.AddStraightLine(starightLine);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);

        }

        // PUT api/straightlinepurchase/5
        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]StraightLine starightLine)
        {
            if (ModelState.IsValid && SetUser(starightLine))
            {
                try
                {
                    _purchaseSvc.UpdateStraightLine(starightLine);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // DELETE api/straightlinepurchase/5
        public void Delete(int id)
        {
        }
    }
}
