using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Blend.WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
               name: "GetStandards",
               routeTemplate: "api/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "GetBuyerContractDetails",
               routeTemplate: "api/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
              name: "GetAddedUsers",
              routeTemplate: "api/{controller}/{id}/{role}",
              defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
             name: "GetAvailableStandards",
             routeTemplate: "api/{controller}/{id}/{standard}",
             defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
            name: "GetMenuByUserAndMainModule",
            routeTemplate: "api/{controller}/{id}/{mainmodule}",
            defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
           name: "GetStraightLineByBoxLotAndInv",
           routeTemplate: "api/{controller}/{id}/{lot}/{inv}",
           defaults: new { id = RouteParameter.Optional }
          );
            config.Routes.MapHttpRoute(
          name: "GetAllPOItems",
          routeTemplate: "api/{controller}/{id}/{poid}",
          defaults: new { id = RouteParameter.Optional }
         );

            config.Routes.MapHttpRoute(
              name: "GetDefaultUserCompany",
              routeTemplate: "api/{controller}/{action}/{id}",
              defaults: new { id = RouteParameter.Optional }
          );
        }
    }
}
