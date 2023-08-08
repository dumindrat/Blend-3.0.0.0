using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessService.TeaStandards;
using Blend.WebApp.App.ActionFilters;

namespace Blend.WebApp.Controllers.Tea.TeaStandard
{
    public class TeaStandardController : BaseApi
    {
        private ITeaStandardService _teaStandardsvc;
        public TeaStandardController(ITeaStandardService teaStandardsvc)
        {
            _teaStandardsvc = teaStandardsvc;
        }

        [HttpGet]
        [ActionName("GetAllStandards")]
        public IEnumerable<Blend.BuisnessEntities.Standard.TeaStandard> GetAllStandards(string id)
        {
            IList<Blend.BuisnessEntities.Standard.TeaStandard> standards = new List<Blend.BuisnessEntities.Standard.TeaStandard>();

            if (ModelState.IsValid)
            {
                try
                {
                    standards = _teaStandardsvc.GetAllTeaStandard(id);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message, "Error");
                }
            }
            return standards;
        }

        // GET api/teastandard/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/teastandard
        [DataOperationAuthorization(operationType = "Create")]
        public HttpResponseMessage Post([FromBody]Blend.BuisnessEntities.Standard.TeaStandard value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _teaStandardsvc.CreateStandard(value);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.LogWrite(ex.Message,"Error");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [DataOperationAuthorization(operationType = "Update")]
        public HttpResponseMessage Put([FromBody]Blend.BuisnessEntities.Standard.TeaStandard value)
        {
            if (ModelState.IsValid && SetUser(value))
            {
                try
                {
                    _teaStandardsvc.UpdateStandard(value);
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

        [DataOperationAuthorization(operationType = "Delete")]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _teaStandardsvc.Deletetandard(new Blend.BuisnessEntities.Standard.TeaStandard() { StandardId = id });
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
    }
}
