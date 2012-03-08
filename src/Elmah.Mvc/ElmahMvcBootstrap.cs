namespace Elmah
{
    #region Imports
    using System.Web.Mvc;
    using System.Web.Routing;
    #endregion

    public class ElmahMvcBootstrap
    {
        public static void Initialize()
        {
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            var constraints = new RouteValueDictionary(new { elmahSecurityConstraint = new ElmahSecurityConstraint() });
            var namespaces = new RouteValueDictionary(new[] { "Elmah" });
            var routeHandler = new MvcRouteHandler();

            var elmahRoute = new Route("elmah/{resource}",
                                       new RouteValueDictionary(
                                           new
                                           {
                                               controller = "Elmah",
                                               action = "Index",
                                               resource = UrlParameter.Optional
                                           }),
                                       constraints,
                                       namespaces,
                                       routeHandler);

            RouteTable.Routes.Insert(0, elmahRoute);
            elmahRoute = new Route("elmah/detail/{resource}",
                                   new RouteValueDictionary(
                                       new { controller = "Elmah", action = "Detail", resource = UrlParameter.Optional }),
                                   constraints,
                                   namespaces,
                                   routeHandler);
            RouteTable.Routes.Insert(0, elmahRoute);
        }
    }
}
