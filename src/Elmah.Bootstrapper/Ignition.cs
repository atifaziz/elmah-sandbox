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

[assembly: System.Web.PreApplicationStartMethod(typeof(Elmah.Bootstrapper.Ignition), "Start")]

namespace Elmah.Bootstrapper
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using Elmah.Assertions;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    #endregion

    public static class Ignition
    {
        private static readonly object _lock = new object();
        private static bool _registered;

        public static void Start()
        {
            lock (_lock)
            {
                if (_registered)
                    return;
                StartImpl();
                _registered = true;
            }
        }

        private static void StartImpl()
        {
            // TODO Consider what happens if registration fails halfway

            ServiceCenter.Current = GetServiceProvider;

            foreach (var type in DefaultModuleTypeSet)
                DynamicModuleUtility.RegisterModule(type);
        }

        private static IEnumerable<Type> DefaultModuleTypeSet
        {
            get
            {
                yield return typeof(ErrorLogModule);
                yield return typeof(ErrorMailModule);
                yield return typeof(ErrorFilterModule);
                yield return typeof(ErrorTweetModule);
                yield return typeof(ErrorLogHandlerMappingModule);
            }
        }

        public static IServiceProvider GetServiceProvider(object context)
        {
            return GetServiceProvider(AsHttpContextBase(context));
        }

        private static HttpContextBase AsHttpContextBase(object context)
        {
            if (context == null)
                return null;
            var httpContextBase = context as HttpContextBase;
            if (httpContextBase != null)
                return httpContextBase;
            var httpContext = context as HttpContext;
            return httpContext == null
                 ? null
                 : new HttpContextWrapper(httpContext);
        }

        private static readonly object _contextKey = new object();

        private static IServiceProvider GetServiceProvider(HttpContextBase context)
        {
            if (context != null)
            {
                var sp = context.Items[_contextKey] as IServiceProvider;
                if (sp != null)
                    return sp;
            }
            
            var container = new ServiceContainer(ServiceCenter.Default(context));

            if (context != null)
            {
                var logPath = context.Server.MapPath("~/App_Data/errors/xmlstore");

                container.AddService(typeof (ErrorLog), delegate
                {
                    return Directory.Exists(logPath) 
                         ? new XmlFileErrorLog(logPath) 
                         : (object) new MemoryErrorLog();
                });

                context.Items[_contextKey] = container;
            }

            return container;
        }
    }

    /*
    class ErrorFilterModule : Elmah.ErrorFilterModule
    {
        protected override void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args)
        {
            base.OnErrorModuleFiltering(sender, args);

            if (args.Dismissed)
                return;

            var type = BuildManager.GetType("", false, true);
            var assertion = Activator.CreateInstance(type) as IAssertion;
            if (assertion == null)
                return;
            if (assertion.Test(args.Context))
                args.Dismiss();
        }
    }*/
}
