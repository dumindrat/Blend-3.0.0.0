







using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Blend.BuisnessEntities.User;
using Blend.BuisnessService.User;
using Blend.Data;

namespace Blend.WebApp.Controllers.Authentication
{
    public class ChangePasswordController : BaseApi
    {
        private IUserService _userService;

        public ChangePasswordController(IUserService userervice)
        {
            _userService = userervice;

        }

        public HttpResponseMessage Post([FromBody]ChangeUserPassword obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    var authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(obj.Password));
                    var credentials = authHeaderValue.Split(':');

                    var status = _userService.Authenticate(new BuisnessEntities.UserEntity { Password = credentials[1], UserName = obj.User });
                    if (status.IsAuthenticated)
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };
                }
                catch (Exception ex)
                {
                    logger.LogWrite("Authentication error " + ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        public HttpResponseMessage Put([FromBody]ChangeUserPassword obj)
        {
            if (ModelState.IsValid && SetUser(obj))
            {
                try
                {
                    var authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(obj.Password));
                    var credentials = authHeaderValue.Split(':');

                    var status = _userService.ChangePassword(obj.User, Blend.BuisnessService.Token.EncryptData.EncryptText(credentials[1]));
                    if (status)
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };
                }
                catch (Exception ex)
                {
                    logger.LogWrite("Authentication error " + ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
