using Blend.BuisnessEntities;
using Blend.BuisnessService;
using Blend.BuisnessService.User;
using Blend.Web.Filters;
using Blend.WebApp.LogHelper;
using BusinessServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Blend.webv1.Controllers.Authentication
{
    [ApiAuthenticationFilter]
    public class UserController : ApiController
    {
        LogWriter logger = new LogWriter();
        private IUserService _userService;
        private ITokenServices _tokenService;
        protected string reqheader_sysuser;

        public UserController()
        {

        }
        public UserController(IUserService userervice)
        {
            _userService = userervice;
            _tokenService = new TokenServices();
            reqheader_sysuser = ConfigurationManager.AppSettings["ReqHeader_SysUser"];

        }
        // GET api/user
        public IEnumerable<string> Get()
        {
            return new string[] { "Dumindra", "Dileepa", "Samintha", "Chamith", "Dasitha" };

        }

        [HttpGet]
        [ActionName("GetPasswordExpireInfo")]
        public PasswordExpireInfo GetPasswordExpireInfo(string id)
        {
            PasswordExpireInfo passwordInfo = new PasswordExpireInfo();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _userService.GetUserById(id);
                    var expireDates = (user.PasswordExpireDate - DateTime.Now).TotalDays;
                    passwordInfo.PasswordExpireDate = user.PasswordExpireDate;
                    passwordInfo.ExpirationDays = expireDates;
                    passwordInfo.IsRequireReset = expireDates <= 5 ? true : false;
                }
                catch (Exception ex)
                {
                    // logger.Error(ex.Message);
                    return passwordInfo;
                }

            }
            return passwordInfo;

        }

        [HttpGet]
        [ActionName("GetUserCompanyInfo")]
        public PasswordExpireInfo GetUserCompanyInfo(string id)
        {
            PasswordExpireInfo passwordInfo = new PasswordExpireInfo();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _userService.GetUserById(id);
                    var expireDates = (user.PasswordExpireDate - DateTime.Now).TotalDays;
                    passwordInfo.PasswordExpireDate = user.PasswordExpireDate;
                    passwordInfo.ExpirationDays = expireDates;
                    passwordInfo.IsRequireReset = expireDates <= 5 ? true : false;
                }
                catch (Exception ex)
                {
                    // logger.Error(ex.Message);
                    return passwordInfo;
                }

            }
            return passwordInfo;

        }

        // GET api/user/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/user
        public HttpResponseMessage Post()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    {
                        var basicAuthenticationIdentity = System.Threading.Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
                        if (basicAuthenticationIdentity != null && basicAuthenticationIdentity.UserId > 0)
                        {

                            var userId = basicAuthenticationIdentity.UserId;
                            logger.LogWrite("Login success for user: " + basicAuthenticationIdentity.UserName);

                            return GetAuthToken(userId, basicAuthenticationIdentity.UserName);
                        }
                    }
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                catch (Exception ex)
                {
                    logger.LogWrite("Login error " + ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/user/5
        public HttpResponseMessage Put([FromBody]UserEntity loginObj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(loginObj.CertId));
                    var credentials = authHeaderValue.Split(':');
                    loginObj.UserName = credentials[0];
                    loginObj.Password = credentials[1];
                    return KillToken(loginObj.Token, loginObj.UserName);

                }
                catch (Exception ex)
                {
                    logger.LogWrite("Error in log off ." + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");
        }

        // DELETE api/user/5
        public void Delete(int id)
        {
        }

        /// <summary>
        /// Returns auth token for the validated user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private HttpResponseMessage GetAuthToken(int userId, string userName)
        {
            var token = _tokenService.GenerateToken(userId);
            var response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
            response.Headers.Add(reqheader_sysuser, userName);
            response.Headers.Add("Token", token.AuthToken);
            response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["AuthTokenExpiry"]);
            response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        private HttpResponseMessage KillToken(string tokenId, string userName)
        {
            var token = _tokenService.Kill(tokenId, userName);
            var response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
            response.Headers.Add("Token", string.Empty);
            response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["AuthTokenExpiry"]);
            response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");

            return response;
        }
    }
}
