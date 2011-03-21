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
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    using Elmah.Assertions;
    using System.Diagnostics;

    #endregion

    public class ThrottleAssertion : IAssertion
    {
        private readonly TimeSpan _throttleDelay;
        private readonly bool _traceThrottledExceptions;

        private DateTime timeOfLastUnfilteredException;
        private Exception _previousException;

        public ThrottleAssertion() : this(new TimeSpan(), false)
        {

        }

        public ThrottleAssertion(TimeSpan delay, bool traceThrottledExceptions)
        {
            _throttleDelay = delay;
            _traceThrottledExceptions = traceThrottledExceptions;
        }

        #region IAssertion Members

        public bool Test(object context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            Exception currentException = ((Elmah.ErrorFilterModule.AssertionHelperContext)context).BaseException;
            bool testResult = false;

            if (_previousException != null)
            {
                bool match = TestExceptionMatch(currentException);
                bool throttled = true;

                // If the throttle delay is not specified, this will throttle all repeated exceptions.
                // Otherwise, check to see if the elapsed time exceeds the throttle delay to determine
                // if the exception should be filtered.
                if (_throttleDelay.TotalMilliseconds > 0 &&
                    DateTime.Now.Subtract(timeOfLastUnfilteredException) > _throttleDelay)
                {
                    throttled = false;
                }

                testResult = match && throttled;
            }
            _previousException = currentException;

            // reset throttle delay timer
            if (!testResult)
                timeOfLastUnfilteredException = DateTime.Now;
            
            Trace.WriteIf(testResult, currentException);

            return testResult;
        }

        protected virtual bool TestExceptionMatch(Exception currentException)
        {
            return (
                currentException.Message == _previousException.Message &&
                currentException.Source == _previousException.Source &&
                currentException.TargetSite == _previousException.TargetSite
                );
        }

        #endregion
    }
}
