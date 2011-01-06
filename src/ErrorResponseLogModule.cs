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

namespace Elmah
{
    #region Imports

    using System;
    using System.Linq;
    using System.Web;

    #endregion

    /// <summary>
    /// HTTP module implementation that synthesizes an exception for HTTP 
    /// requests ending in HTTP status code 4xx or 5xx and signals it to 
    /// ELMAH.
    /// </summary>
    /// <remarks>
    /// Consider overriding the virtual members of this module to 
    /// customize behavior. For example, to signal an error only for certain
    /// status codes only, override <see cref="MapException"/>.
    /// </remarks>

    // TODO Consider testing in Medium trust environments

    public class ErrorResponseLogModule : IHttpModule
    {
        private static readonly object _contextKey = new object();

        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (!HttpRuntime.UsingIntegratedPipeline)
            {
                // Otherwise subscribing to LogRequest throws 
                // System.PlatformNotSupportedException: 
                // This operation requires IIS integrated pipeline mode.

                return;
            }

            var modules = context.Modules;
            var errorLogModule = Enumerable.Range(0, modules.Count)
                                           .Select(i => modules[i])
                                           .OfType<ErrorLogModule>()
                                           .SingleOrDefault();

            if (errorLogModule != null)
                errorLogModule.Logged += OnErrorLogged;

            context.LogRequest += (sender, _) => OnLogRequest(((HttpApplication) sender).Context);
        }

        private static void SetError(HttpContext context, Error error)
        {
            context.Items[_contextKey] = error;
        }

        protected Error GetError(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Items[_contextKey] as Error;
        }

        protected virtual void OnErrorLogged(object sender, ErrorLoggedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            SetError(HttpContext.Current, args.Entry.Error);
        }

        protected virtual void OnLogRequest(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (GetError(context) != null)
                return;
            
            Exception e = MapException(context);
            if (e == null)
                return;
            
            SignalRequestException(context, e);
        }

        protected virtual Exception MapException(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var statusCode = context.Response.StatusCode;
            if (statusCode < 400 || statusCode >= 600)
                return null;
            var message = string.Format("HTTP response generated with status code {0} for: {1}", statusCode, context.Request.Path);
            return new HttpSynthesizedException(statusCode, message);
        }

        protected virtual void SignalRequestException(HttpContext context, Exception e)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (e == null) throw new ArgumentNullException("e");
            
            ErrorSignal.FromContext(context).Raise(e, context);
        }

        public void Dispose() { }
    }
}
