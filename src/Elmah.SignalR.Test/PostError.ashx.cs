namespace Elmah.SignalR.Test
{
    // PostError is a handler which expects a base64-encoded
    // json representation of an error. 

    #region Imports

    using System;
    using System.Web;
    using System.Text;
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

            var e = Error.Build(error);

            var envelope = new Envelope
                           {
                               Id = source.Id,
                               ApplicationName = source.ApplicationName, 
                               Error = e, 
                               InfoUrl = infoUrl
                           };

            var connectionManager = AspNetHost.DependencyResolver.Resolve<IConnectionManager>();
            connectionManager.GetClients<ElmahRHub>().notifyErrors(new [] { envelope });

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