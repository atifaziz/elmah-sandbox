namespace Elmah.SignalR.Test
{
    #region Imports

    using System;
    using System.Web;

    #endregion

    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ErrorsStore.BuildSourcesFromConfig(HttpContext.Current);

            //ErrorsStore .Store
            //            .AddSource(
            //            "ElmahR sample erratic application 1",  "The Fool on the Hill")
            //            .AddSource(
            //            "ElmahR sample erratic application 2",  "Lucy in the Sky with Diamonds")
            //            .AddSource(
            //            "ElmahR sample erratic application 3",  "Strawberry Fields Forever");
        }
    }
}