#region License, Terms and Author(s)
//
// ELMAH Sandbox
// Copyright (c) 2010-11 Atif Aziz. All rights reserved.
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
            if (config == null) throw new ArgumentNullException("config");

            var attribute = config.GetAttributeNode("delayTimeSpan");
            var delay = attribute != null
                      ? TimeSpan.Parse(attribute.Value)
                      : TimeSpan.Zero;

            attribute = config.GetAttributeNode("traceThrottledExceptions");
            var trace = attribute != null && bool.Parse(attribute.Value);
            
            return new ThrottleAssertion(delay, trace);
        }
    }
}
