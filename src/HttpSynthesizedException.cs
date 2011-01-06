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
    using System.Runtime.Serialization;
    using System.Web;

    #endregion

    [ Serializable ]
    public class HttpSynthesizedException : HttpException
    {
        public HttpSynthesizedException() {}
        
        public HttpSynthesizedException(string message) : 
            base(message) {}
        
        public HttpSynthesizedException(string message, Exception inner) : 
            base(message, inner) {}
        
        public HttpSynthesizedException(int httpCode, string message, Exception innerException) : 
            base(httpCode, message, innerException) {}
        
        public HttpSynthesizedException(int httpCode, string message)
            : base(httpCode, message) {}
        
        protected HttpSynthesizedException(SerializationInfo info, StreamingContext context) : 
            base(info, context) {}
    }
}