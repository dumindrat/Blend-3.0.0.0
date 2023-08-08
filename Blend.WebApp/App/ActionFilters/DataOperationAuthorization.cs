using Blend.BuisnessService.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Blend.WebApp.App.ActionFilters
{

    public class DataOperationAuthorization : ActionFilterAttribute
    {
        private const string MenuNameHeaderValue = "AccessLink";
        public string operationType { get; set; }

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            //  Get API key provider
            var provider = filterContext.ControllerContext.Configuration
                .DependencyResolver.GetService(typeof(IUserService)) as IUserService;
            if (filterContext.Request.Headers.Contains(MenuNameHeaderValue))
            {
                var accessLink = filterContext.Request.Headers.GetValues(MenuNameHeaderValue).First();
                string authHeaderValue = null;

                var authRequest = filterContext.Request.Headers.Authorization;
                if (authRequest != null && !String.IsNullOrEmpty(authRequest.Scheme) && authRequest.Scheme == "Basic")
                    authHeaderValue = authRequest.Parameter;
                if (!string.IsNullOrEmpty(authHeaderValue))
                {
                    authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(authHeaderValue));
                    var credentials = authHeaderValue.Split(':');

                    // Validate Token
                    try
                    {
                        if (provider != null && !provider.Authorize(credentials[0], operationType, accessLink))
                        {
                            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "No access rights to do this operation" };
                            filterContext.Response = responseMessage;
                        }
                    }
                    catch (Exception ex)
                    {
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Error :" + ex.Message };
                        filterContext.Response = responseMessage;
                    }
                }
                else
                {
                    filterContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            else
            {
                filterContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            base.OnActionExecuting(filterContext);

        }
    }
}