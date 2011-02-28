#region License, Terms and Author(s)
//
// ELMAH Sandbox
// Copyright (c) 2010-11 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
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

namespace Elmah.Bootstrapper
{
    #region Imports

    using System;
    using System.Web;
    using Mannex;

    #endregion

    static class WebExtensions
    {
        /// <summary>
        /// Helps with subscribing to <see cref="HttpApplication"/> events
        /// but where the handler 
        /// </summary>

        public static void Subscribe(this HttpApplication application, 
            Action<EventHandler> subscriber, 
            Action<HttpContextBase> handler)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            if (handler == null) throw new ArgumentNullException("handler");
        
            subscriber((sender, _) => handler(new HttpContextWrapper(((HttpApplication) sender).Context)));
        }

        /// <summary>
        /// Same as <see cref="IHttpHandlerFactory.GetHandler"/> except the
        /// HTTP context is typed as <see cref="HttpContextBase"/> instead
        /// of <see cref="HttpContext"/>.
        /// </summary>

        public static IHttpHandler GetHandler(this IHttpHandlerFactory factory, 
            HttpContextBase context, string requestType, 
            string url, string pathTranslated)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.GetHandler(context.GetRuntimeContext(), requestType, url, pathTranslated);
        }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> object that may be associated
        /// with a given <see cref="HttpContextBase"/> object.
        /// </summary>
        /// <remarks>
        /// An exception is thrown if <paramref name="context"/> does not
        /// support a query for <see cref="HttpApplication"/> as a service.
        /// </remarks>

        public static HttpContext GetRuntimeContext(this HttpContextBase context)
        {
            if (context == null) throw new ArgumentNullException("context");            
            // http://stackoverflow.com/questions/1992141/how-do-i-get-an-httpcontext-object-from-httpcontextbase-in-asp-net-mvc-1/4567707#4567707
            return context.GetRequiredService<HttpApplication>().Context;
        }
    }
}