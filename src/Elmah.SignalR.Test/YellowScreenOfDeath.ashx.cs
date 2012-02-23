namespace Elmah.SignalR.Test
{
    #region Imports

    using System.Web;

    #endregion

    /// <summary>
    /// Summary description for YellowScreenOfDeath
    /// </summary>
    public class YellowScreenOfDeath : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var errorId = context.Request.QueryString["id"] ?? string.Empty;

            if (errorId.Length == 0)
                return;

            var response = context.Response;

            var error = ErrorsStore.Store.GetError(errorId);
            if (error == null)
            {
                // TODO: Send error response entity
                response.Status = "404 Not Found";
                return;
            }

            if (error.WebHostHtmlMessage.Length == 0)
                return;

            response.Write(error.WebHostHtmlMessage);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}