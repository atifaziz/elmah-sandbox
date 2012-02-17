namespace Elmah.SignalR.Test
{
    #region Imports

    using global::SignalR.Hubs;
    using System.Linq;

    #endregion

    // Elmah-SignalR hub

    [HubName("elmahr")]
    public class ElmahRHub : Hub
    {
        public void Connect()
        {
            var envs = from source in ErrorsStore.Store
                       from error in source
                       select new Envelope
                       {
                           id = source.Id,
                           applicationName = source.ApplicationName,
                           error = error,
                           infoUrl = source.InfoUrl
                       };

            foreach (var env in envs)
                Caller.notifyError(env);
        }
    }
}