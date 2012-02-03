using System;
using System.Web;
using System.Text;
using System.Web.Script.Serialization;
using SignalR.Hubs;

namespace Elmah.SignalR.Test
{
    // PostError is a handler which expects a base64-encoded
    // json representation of an error. 

    public class PostError : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var error = Decode(context.Request.Params["error"]);

            var js = new JavaScriptSerializer();

            var e = js.Deserialize<Error>(error);

            Hub.GetClients<ElmahRHub>().notifyError(e);
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