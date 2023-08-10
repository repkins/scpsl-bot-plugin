using Mirror;

namespace TestPlugin.LocalNetworking
{
    internal static class NetworkIdentityExtensions
    {
        internal static void RemoveObserver(this NetworkIdentity identity, NetworkConnection conn)
        {
            identity.observers.Remove(conn.connectionId);
        }
    }
}
