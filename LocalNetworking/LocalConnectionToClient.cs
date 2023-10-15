using Mirror;
using System;

namespace SCPSLBot.LocalNetworking
{
    public class LocalConnectionToClient : NetworkConnectionToClient
    {
        public LocalConnectionToClient(int connectionId) : base(connectionId)
        {
        }

        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
            //NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
            //networkWriterPooled.WriteBytes(segment.Array, segment.Offset, segment.Count);
            //this.connectionToServer.queue.Enqueue(networkWriterPooled);
        }

        internal bool IsAlive(float timeout)
        {
            return true;
        }

        internal void DisconnectInternal()
        {
            isReady = false;
            this.RemoveFromObservingsObservers();
        }

        public override void Disconnect()
        {
            DisconnectInternal();
            connectionToServer.DisconnectInternal();
        }

        internal LocalConnectionToServer connectionToServer;
    }
}
