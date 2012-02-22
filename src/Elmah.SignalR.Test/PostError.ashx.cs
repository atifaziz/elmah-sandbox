using System.Text.RegularExpressions;

namespace Elmah.SignalR.Test
{
    // PostError is a handler which expects a base64-encoded
    // json representation of an error. 

    #region Imports

    using System;
    using System.Web;
    using System.Text;
    using System.Web.Script.Serialization;
    using global::SignalR;
    using global::SignalR.Hosting.AspNet;
    using global::SignalR.Infrastructure;

    #endregion

    public class PostError : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var error           = Decode(context.Request.Params["error"]);
            var errorId         = context.Request.Params["errorId"];
            var handshakeToken  = context.Request.Params["handshakeToken"];
            var infoUrl         = context.Request.Params["infoUrl"];

            var source = ErrorsStore.Store[handshakeToken];
            if (source == null) 
                return;

            source.SetInfoUrl(infoUrl);

            var js = new JavaScriptSerializer();

            var e = js.Deserialize<Error>(error);

            var match = Regex.Match(e.type, @"(\w+\.)+(?'type'\w+)Exception");
            e.shortType = match.Success 
                          ? match.Groups["type"].Value 
                          : e.type;

            var browserSupportUrl = "http://www.w3schools.com/images/{0}.gif";

            if (e.serverVariables.ContainsKey("HTTP_USER_AGENT"))
            {
                var userAgent = e.serverVariables["HTTP_USER_AGENT"];
                if (userAgent.IndexOf("MSIE", StringComparison.OrdinalIgnoreCase) >= 0)
                    e.browserSupportUrl = string.Format(browserSupportUrl, "compatible_ie");
                else if (userAgent.IndexOf("Chrome", StringComparison.OrdinalIgnoreCase) >= 0)
                    e.browserSupportUrl = string.Format(browserSupportUrl, "compatible_chrome");
                else if (userAgent.IndexOf("Firefox", StringComparison.OrdinalIgnoreCase) >= 0)
                    e.browserSupportUrl = string.Format(browserSupportUrl, "compatible_firefox");
                else if (userAgent.IndexOf("Safari", StringComparison.OrdinalIgnoreCase) >= 0)
                    e.browserSupportUrl = string.Format(browserSupportUrl, "compatible_safari");
                else if (userAgent.IndexOf("Opera", StringComparison.OrdinalIgnoreCase) >= 0)
                    e.browserSupportUrl = string.Format(browserSupportUrl, "compatible_opera");
            }

            e.isoTime = e.time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var a = new Envelope {id = source.Id, applicationName = source.ApplicationName, error = e, infoUrl = infoUrl};

            var connectionManager = AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            connectionManager.GetClients<ElmahRHub>().notifyError(a);

            source.AppendError(e, errorId);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        static string Decode(string str)
        {
            var decbuff = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(decbuff);
        }
    }
}