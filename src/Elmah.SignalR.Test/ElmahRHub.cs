namespace Elmah.SignalR.Test
{
    #region Imports

    using global::SignalR.Hubs;

    #endregion

    // Elmah-SignalR hub

    [HubName("elmahr")]
    public class ElmahRHub : Hub
    {
    }
}