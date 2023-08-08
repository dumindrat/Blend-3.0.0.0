using Blend.Data.Response;
using Blend.WebApp.App.ActionFilters;
using Blend.Web.Filters;
using Blend.WebApp.LogHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Blend.WebApp.Controllers
{
    [AuthorizationRequired]
    [DataOperationAuthorization(operationType = "View")]
    public class BaseApi : ApiController
    {
        protected string success = "Success";
        protected string error = "Error";
        protected ResponseData response;
        protected string currentUser;
        protected string sys_ver;
        protected string reqheader_sysuser;
        protected LogWriter logger = new LogWriter();

        public BaseApi()
        {
            response = new ResponseData { Messages = new List<string>() };
            var basicAuthenticationIdentity = System.Threading.Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
            sys_ver = ConfigurationManager.AppSettings["sys_ver"];
            reqheader_sysuser = ConfigurationManager.AppSettings["ReqHeader_SysUser"];
        }

        protected string GetUser()
        {
            return Request.Headers.GetValues(reqheader_sysuser).FirstOrDefault();
        }

        protected bool SetUser(Blend.BuisnessEntities.BaseEntity obj)
        {
            try
            {
                IEnumerable<string> headerValues = Request.Headers.GetValues(reqheader_sysuser);
                var user = headerValues.FirstOrDefault();
                if (string.IsNullOrEmpty(user))
                {
                    logger.LogWrite("User couldnt find in request", "Error");
                    return false;
                }
                else
                {
                    obj.User = user;
                }
            }
            catch (Exception ex)
            {
                logger.LogWrite("Error : " + ex.Message, "Error");
                return false;
            }
            return true;
        }
    }
}