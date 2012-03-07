#region License, Terms and Author(s)
//
// ELMAH.Mvc3
// Copyright (c) 2011 Atif Aziz, James Driscoll. All rights reserved.
//
//  Author(s):
//
//      James Driscoll
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace Elmah
{
    #region Imports

    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    #endregion

    public class ErrorMvc3Module : HttpModuleBase
    {
        protected override void OnInit(HttpApplication application)
        {
            base.OnInit(application);

            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            var constraints = new RouteValueDictionary(new {elmahSecurityConstraint = new ElmahSecurityConstraint()});
            var namespaces = new RouteValueDictionary(new[] {"Elmah"});
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
                                       new {controller = "Elmah", action = "Detail", resource = UrlParameter.Optional}),
                                   constraints,
                                   namespaces,
                                   routeHandler);
            RouteTable.Routes.Insert(0, elmahRoute);
        }

        protected override bool SupportDiscoverability
        {
            get { return true; }
        }
    }
}