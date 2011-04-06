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
    using Elmah.Assertions;
    using System.Diagnostics;

    #endregion

    public class ThrottleAssertion : IAssertion
    {
        private readonly TimeSpan _throttleDelay;
        private readonly bool _traceThrottledExceptions;
        private DateTime _timeOfLastUnfilteredException;
        private Exception _previousException;

        public ThrottleAssertion() : 
            this(TimeSpan.Zero, false) {}

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

            var currentException = ((ErrorFilterModule.AssertionHelperContext) context).BaseException;

            // If the throttle delay is not specified, this will throttle all repeated exceptions.
            // Otherwise, check to see if the elapsed time exceeds the throttle delay to determine
            // if the exception should be filtered.

            var result = _previousException != null 
                         && TestExceptionMatch(currentException, _previousException) 
                         && (_throttleDelay == TimeSpan.Zero 
                             || DateTime.Now - _timeOfLastUnfilteredException <= _throttleDelay);
            
            _previousException = currentException;

            // reset throttle delay timer
            if (!result)
                _timeOfLastUnfilteredException = DateTime.Now;

            if (_traceThrottledExceptions)
                Trace.WriteIf(result, currentException);

            return result;
        }

        protected virtual bool TestExceptionMatch(Exception currentException, Exception previousException)
        {
            return currentException.Message    == previousException.Message 
                && currentException.Source     == previousException.Source 
                && currentException.TargetSite == previousException.TargetSite;
        }

        #endregion
    }
}
