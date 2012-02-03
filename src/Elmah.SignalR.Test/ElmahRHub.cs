namespace Elmah.SignalR.Test
{
    #region Imports

    using global::SignalR.Hubs;

    #endregion

    // Elmah-SignalR hub

    [HubName("elmahr")]
    public class ElmahRHub : Hub
    {
        public void Send(Error error)
        {
            // Call the 'notifyError' method on all clients
            Clients.notifyError(error);
        }
    }
}