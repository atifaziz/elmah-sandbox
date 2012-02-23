namespace Elmah.SignalR.Test
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;

    #endregion

    public class Error
    {
        const string BrowserSupportUrlTemplate = "http://www.w3schools.com/images/{0}.gif";

        public string Host { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public string StatusCode { get; set; }
        public string WebHostHtmlMessage { get; set; }

        public IDictionary<string, string> ServerVariables { get; set; }
        public IDictionary<string, string> Form { get; set; }
        public IDictionary<string, string> Cookies { get; set; }

        public DateTime Time { get; set; }

        public string Url { get; set; }

        public string ShortType { get; set; }
        public string BrowserSupportUrl { get; set; }
        public string IsoTime { get; set; }
        public bool HasYsod { get; set; }

        public static Error Build(string error)
        {
            var js = new JavaScriptSerializer();

            var e = js.Deserialize<Error>(error);

            var match = Regex.Match(e.Type, @"(\w+\.)+(?'type'\w+)Exception");
            e.ShortType = match.Success
                        ? match.Groups["type"].Value
                        : e.Type;

            if (e.ServerVariables.ContainsKey("HTTP_USER_AGENT"))
            {
                var userAgent = e.ServerVariables["HTTP_USER_AGENT"];

                e.BrowserSupportUrl = userAgent.IndexOf("MSIE", StringComparison.OrdinalIgnoreCase) >= 0
                                    ? string.Format(BrowserSupportUrlTemplate, "compatible_ie")
                                    : userAgent.IndexOf("Chrome", StringComparison.OrdinalIgnoreCase) >= 0
                                    ? string.Format(BrowserSupportUrlTemplate, "compatible_chrome")
                                    : userAgent.IndexOf("Safari", StringComparison.OrdinalIgnoreCase) >= 0
                                    ? string.Format(BrowserSupportUrlTemplate, "compatible_safari")
                                    : userAgent.IndexOf("Opera", StringComparison.OrdinalIgnoreCase) >= 0
                                    ? string.Format(BrowserSupportUrlTemplate, "compatible_opera")
                                    : string.Empty;
            }

            e.IsoTime = e.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            return e;
        }
    }
}