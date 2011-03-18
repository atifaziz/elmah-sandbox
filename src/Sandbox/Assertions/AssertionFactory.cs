#region License, Terms and Author(s)
//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-11 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Jesse Arnold
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


namespace Elmah.Sandbox.Assertions
{
    #region Imports

    using System;
    using System.Xml;
    using Elmah.Assertions;

    #endregion

    public sealed class AssertionFactory
    {

        public static IAssertion assert_throttle(XmlElement config)
        {
            TimeSpan delay = new TimeSpan();
            XmlAttribute attribute = config.GetAttributeNode("delayTimeSpan");
            if (attribute != null)
            {
                delay = TimeSpan.Parse(attribute.Value);
            }

            bool trace = false;
            attribute = config.GetAttributeNode("traceThrottledExceptions");
            if (attribute != null)
            {
                trace = bool.Parse(attribute.Value);
            }

            return new ThrottleAssertion(delay, trace);
        }
    }
}
