#region License, Terms and Author(s)
//
// ELMAH.Mvc3
// Copyright (c) 2011 Atif Aziz, James Driscoll. All rights reserved.
//
//  Author(s):
//
//      Darren Weir, http://dotnetdarren.wordpress.com/2010/07/27/logging-on-mvc-part-1/
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
    using System.Web.Mvc;
    using System.Web.Routing;
    #endregion

    public class ErrorHandlingControllerFactory : DefaultControllerFactory
    {
        private readonly IControllerFactory _originalControllerFactory;

        public ErrorHandlingControllerFactory()
        {
            var originalControllerFactory = ControllerBuilder.Current.GetControllerFactory();
            if (originalControllerFactory.GetType() != GetType())
                _originalControllerFactory = originalControllerFactory;
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            var controller = _originalControllerFactory == null ?
                                base.CreateController(requestContext, controllerName) :
                                _originalControllerFactory.CreateController(requestContext, controllerName);

            var c = controller as Controller;
            if (c != null)
                c.ActionInvoker = new ErrorHandlingActionInvoker(new HandleErrorWithElmahAttribute());

            return controller;
        }
    }
}
