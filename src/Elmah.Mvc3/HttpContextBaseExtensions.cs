#region License, Terms and Author(s)
//
// ELMAH.Mvc3
// Copyright (c) 2011 Atif Aziz, James Driscoll. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
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
    using System;
    using System.Web;

    static class HttpContextBaseExtensions
    {
        public static HttpContext GetImplementation(this HttpContextBase context)
        {
            if (context == null) throw new ArgumentNullException("context");
            // http://stackoverflow.com/questions/1992141/how-do-i-get-an-httpcontext-object-from-httpcontextbase-in-asp-net-mvc-1/4567707#4567707
            var application = (HttpApplication) context.GetService(typeof(HttpApplication));
            if (application == null)
                throw new Exception(string.Format("Service {0} is not available from given context.", typeof(HttpApplication)));
            return application.Context;
        }
    }
}