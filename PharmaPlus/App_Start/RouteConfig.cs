using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PharmaPlus
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // About route
            routes.MapRoute(
                name: "About",
                url: "About",
                defaults: new { controller = "About", action = "Index" }
            );

            // Checkout route
            routes.MapRoute(
                name: "Checkout",
                url: "Checkout/{action}/{id}",
                defaults: new { controller = "Checkout", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
