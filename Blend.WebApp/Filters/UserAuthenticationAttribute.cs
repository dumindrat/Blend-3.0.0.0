using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Blend.Web.Filters
{
    public class UserAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            //{
            //    //actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            //    //{
            //    //    ReasonPhrase = "Unautherized access"
            //    //};
            //}
        }
    }
}