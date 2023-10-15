using Mirror;
using TestPlugin.LocalNetworking;

namespace SCPSLBot.LocalNetworking
{
    public static class NetworkConnectionExtensions
    {
        public static void RemoveFromObservingsObservers(this NetworkConnectionToClient connection)
        {
            foreach (NetworkIdentity networkIdentity in connection.observing)
            {
                networkIdentity.RemoveObserver(connection);
            }
            connection.observing.Clear();
        }
    }
}
