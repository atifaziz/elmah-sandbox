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
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Web;
    using System.Web.Routing;
    using System.Web.Security;
    #endregion

    class ElmahSecurityConstraint : IRouteConstraint
    {
        public ElmahSecurityConstraint()
        {
            var allowedRoles = ConfigurationManager.AppSettings["elmah$mvc$allowedRoles"] ?? string.Empty;
            _allowedRoles = allowedRoles.Split(',')
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .ToArray();
            var isHandlerEnabled = ConfigurationManager.AppSettings["elmah$mvc$enableHandler"];
            if (string.IsNullOrEmpty(isHandlerEnabled))
                isHandlerEnabled = "true";

            bool.TryParse(isHandlerEnabled, out _isHandlerEnabled);
        }

        private readonly string[] _allowedRoles;
        private readonly bool _isHandlerEnabled;
            
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!_isHandlerEnabled)
                return false;

            if (_allowedRoles == null || _allowedRoles.Length == 0)
                return true;

            return httpContext.Request.IsAuthenticated && _allowedRoles.Any(httpContext.User.IsInRole);
        }
    }
}
