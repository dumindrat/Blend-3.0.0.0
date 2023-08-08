using System.Threading;
using System.Web.Http.Controllers;
using BusinessServices;
using Blend.BuisnessService.User;
using Blend.BuisnessEntities;
using Blend.WebApp.LogHelper;

namespace Blend.Web.Filters
{
    /// <summary>
    /// Custom Authentication Filter Extending basic Authentication
    /// </summary>
    public class ApiAuthenticationFilter : GenericAuthenticationFilter
    {
        LogWriter logger = new LogWriter();

        /// <summary>
        /// Default Authentication Constructor
        /// </summary>
        public ApiAuthenticationFilter()
        {
        }

        /// <summary>
        /// AuthenticationFilter constructor with isActive parameter
        /// </summary>
        /// <param name="isActive"></param>
        public ApiAuthenticationFilter(bool isActive)
            : base(isActive)
        {
        }

        /// <summary>
        /// Protected overriden method for authorizing user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            var provider = actionContext.ControllerContext.Configuration
                               .DependencyResolver.GetService(typeof(IUserService)) as IUserService;
            
            if (provider != null)
            {
                logger.LogWrite("Service providr found on [OnAuthorizeUser]");
                var user = provider.Authenticate(new UserEntity { UserName = username, Password = password });
                if (user.IsAuthenticated)
                {
                    var basicAuthenticationIdentity = Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
                    if (basicAuthenticationIdentity != null)
                        basicAuthenticationIdentity.UserId = user.UserId;
                    return true;
                }
            }
            return false;
        }
    }
}