using Blend.BuisnessEntities.User;
using Blend.BuisnessService.User;
using Blend.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Blend.WebApp.Controllers.Authentication
{
    public class UserRegistrationController : ApiController
    {
        private IUserRegistration _regService;

        public UserRegistrationController(IUserRegistration regService)
        {
            _regService = regService;

        }

        // GET api/userregistration
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/userregistration/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/userregistration
        public HttpResponseMessage Post([FromBody]UserRegistration regObj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _regService.CreateUser(regObj);
                    return Request.CreateResponse(HttpStatusCode.OK, regObj);

                }
                catch (Exception ex)
                {
                    //logger.Error(ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");

        }

        // PUT api/userregistration/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/userregistration/5
        public void Delete(int id)
        {
        }
    }
}
