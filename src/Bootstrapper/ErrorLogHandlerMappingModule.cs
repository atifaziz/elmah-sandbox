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
    using System.Text.RegularExpressions;
    using System.Web;

    #endregion

    sealed class ErrorLogHandlerMappingModule : HttpModuleBase
    {
        private ErrorLogPageFactory _errorLogPageFactory;

        private ErrorLogPageFactory HandlerFactory
        {
            get { return _errorLogPageFactory ?? (_errorLogPageFactory = new ErrorLogPageFactory()); }
        }

        protected override void OnInit(HttpApplication application)
        {
            application.Subscribe(h => application.PostMapRequestHandler += h, OnPostMapRequestHandler);
            application.Subscribe(h => application.EndRequest += h, OnEndRequest);
        }

        private void OnPostMapRequestHandler(HttpContextBase context)
        {
            var request = context.Request;
            
            var url = request.FilePath;
            if (!Regex.IsMatch(url, @"(?i:\belmah\b)", RegexOptions.CultureInvariant))
                return;
            
            var pathTranslated = request.PhysicalApplicationPath;
            var factory = HandlerFactory;
            var handler = factory.GetHandler(context, request.HttpMethod, url, pathTranslated);
            if (handler == null) 
                return;
            
            context.Items[this] = new ContextState
            {
                Handler = handler,
                HandlerFactory = factory,
            };
            
            context.Handler = handler;
        }

        private void OnEndRequest(HttpContextBase context)
        {
            var state = context.Items[this] as ContextState;
            if (state == null)
                return;
            state.HandlerFactory.ReleaseHandler(state.Handler);
        }
        
        sealed class ContextState
        {
            public IHttpHandler Handler;
            public IHttpHandlerFactory HandlerFactory;
        }
    }
}