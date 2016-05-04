using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("ExternalPageRoute", "E/{NavigationId}/{LangId}",
               new
               {
                   controller = "Page",
                   action = "ExternalBuildPage"
               }

           );

            routes.MapRoute("PageRoute", "P/{NavigationId}/{LangId}",
                new
                {
                    controller = "Page",
                    action = "BuildPage"
                }
            );
            routes.MapRoute("ErrorNotFound", "Error/HomePage/{LangId}",
                new
                {
                    controller = "Error",
                    action = "NotFound"
                }
            );
            routes.MapRoute("PersonelRoute", "Page/PersonelInfo/{LangId}",
               new
               {
                   controller = "Page",
                   action = "PersonelInfo"
               }
           );
            routes.MapRoute("PageRouteExternalWidget", "Page/RenderExternalWidget/{PortalId}/{WidgetId}",
               new
               {
                   controller = "Page",
                   action = "RenderExternalWidget"
               }
           );
            routes.MapRoute("ExternalPageWithLogin", "EWL/{PageId}/{LangId}",
               new
               {
                   controller = "External",
                   action = "WithLogin"
               }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
