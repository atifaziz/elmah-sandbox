using System;
using System.Web;

namespace Elmah.SignalR.Test
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ErrorsStore.Store
                .AddSource(
                    "Wasp is doing ElmahR", 
                    "The fool on the hill")
                .AddSource(
                    "Wasp is doing ElmahR with self-hosting",
                    "Lucy in the Sky with Diamonds")
                .AddSource(
                    "Wasp is doing ElmahR again",
                    "Strawberry fields forever");
        }

    }
}