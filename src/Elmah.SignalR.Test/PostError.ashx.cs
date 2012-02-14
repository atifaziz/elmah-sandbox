using System;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.Web.Script.Serialization;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Hubs;
using SignalR.Infrastructure;

namespace Elmah.SignalR.Test
{
    // PostError is a handler which expects a base64-encoded
    // json representation of an error. 

    public class PostError : IHttpHandler
    {
        static readonly HashAlgorithm _hash = new MD5CryptoServiceProvider();

        string HashData(string data)
        {
            byte[] bytes = (new UnicodeEncoding()).GetBytes(data);
            byte[] hashed = _hash.ComputeHash(bytes);
            var sb = new StringBuilder(64);
            foreach (var b in hashed)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

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