#region License, Terms and Author(s)
//
// ELMAH Sandbox
// Copyright (c) 2010-11 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      http://code.google.com/u/miketrionfo/
//      Atif Aziz, http://www.raboof.com (refactored)
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Web;

    #endregion

    /// <remarks>
    /// Only IPv4 addressing is supported.
    /// </remarks>

    public sealed class NetworkAuthorizationModule : HttpModuleBase, IRequestAuthorizationHandler
    {
        private Predicate<IPAddress> _isAuthorized = Unauthorize;

        protected override void OnInit(HttpApplication application)
        {
            Init(GetConfig());
        }

        protected override bool SupportDiscoverability { get { return true; } }

        private static IDictionary GetConfig()
        {
            #pragma warning disable 612,618
            // NOTE! This may not be future proof
            return (IDictionary) ConfigurationSettings.GetConfig("elmah/security");
            #pragma warning restore 612,618
        }

        public bool Authorize(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return IsAuthorized(IPAddress.Parse(context.Request.UserHostAddress));
        }

        public bool IsAuthorized(IPAddress remoteAddress)
        {
            if (remoteAddress == null) throw new ArgumentNullException("remoteAddress");
            return _isAuthorized(remoteAddress);
        }

        private void Init(IDictionary options)
        {
            // Check to see if the networks are listed
            var networksString = GetString(options, "allowedNetworks").Trim();

            // Split out the networks allowed i.e. 192.168.36.*
            var networks = networksString.Split(',', '|', ';')
                                         .Select(m => m.Trim())
                                         .Where(m => m.Length >= 0)
                                         .ToArray();

            // Now see if the requsted address falls within any of the masks specified in "allowedNetworks"
            _isAuthorized = ParseNetworkMembershipCheck(networks);
        }

        private static bool Unauthorize(IPAddress address) { return false; }
        
        public static Predicate<IPAddress> ParseNetworkMembershipCheck(IEnumerable<string> specifications)
        {
            if (specifications == null) throw new ArgumentNullException("specifications");

            var seed = (Predicate<IPAddress>) null;
            var predicate = specifications.Select(ParseNetworkMembershipCheck)
                                 .Aggregate(seed, (acc, p) => acc == null ? p : address => acc(address) || p(address));
            return predicate ?? Unauthorize;
        }

        public static Predicate<IPAddress> ParseNetworkMembershipCheck(string specification)
        {
            var mask = ParseMask(specification);
            var network = IPAddress.Parse(specification.Replace('*', '0'));

            return address => address.AddressFamily == AddressFamily.InterNetwork // IPv4 only!
                           && address.GetNetworkAddress(mask).Equals(network);
        }

        private static IPAddress ParseMask(string specification)
        {
            if (specification == null) throw new ArgumentNullException("specification");

            // TODO Consider also adding support for CIDR notation
            var parts = specification.Split('.');
            if (parts.Length != 4) // TODO Consider Regex validation
                throw new FormatException("Invalid network specification. Use notation like 192.168.*.* or 192.168.3.*, etc.");

            return new IPAddress(parts.Select(p => IsStar(p) ? (byte) 0 : (byte) 255).ToArray());
        }

        private static bool IsStar(string value)
        {
            return value != null && value.Length == 1 && value[0] == '*';
        }

        private static string GetString(IDictionary options, string name)
        {
            Debug.Assert(name != null);
            return options == null 
                 ? string.Empty 
                 : ((options[name] ?? string.Empty).ToString() ?? string.Empty);
        }
    }
}
