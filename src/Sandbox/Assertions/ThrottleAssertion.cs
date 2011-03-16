using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Elmah.Assertions
{
    public class ThrottleAssertion : IAssertion
    {
        private DateTime timeOfLastUnfilteredException;

        public ThrottleAssertion()
        {

        }

        public ThrottleAssertion(int delay)
        {
            _throttleDelay = delay;
        }

        private Exception _previousException;

        public Exception PreviousException
        {
            get { return _previousException; }
        }

        private int _throttleDelay;

        public int ThrottleDelay {
            get
            {
                return _throttleDelay;
            }
        }

        #region IAssertion Members

        public bool Test(object context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            Exception currentException = ((Elmah.ErrorFilterModule.AssertionHelperContext)context).BaseException;
            bool result = false;

            if (_previousException != null)
            {
                bool match = TestExceptionMatch(currentException);
                bool throttled = true;

                if (_throttleDelay > 0 &&
                    DateTime.Now.Subtract(timeOfLastUnfilteredException).TotalMinutes > _throttleDelay)
                {
                    throttled = false;
                }

                result = match && throttled;
            }
            _previousException = currentException;

            // reset throttle delay timer
            if (!result)
                timeOfLastUnfilteredException = DateTime.Now;

            return result;
        }

        private bool TestExceptionMatch(Exception currentException)
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
