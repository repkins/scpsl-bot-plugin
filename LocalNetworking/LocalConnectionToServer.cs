using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace TestPlugin.LocalNetworking
{
    internal class LocalConnectionToServer : NetworkConnectionToServer
    {
        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
            if (segment.Count == 0)
            {
                Debug.LogError("LocalConnection.SendBytes cannot send zero bytes");
                return;
            }
            Batcher batchForChannelId = GetBatchForChannelId(channelId);
            batchForChannelId.AddMessage(segment, NetworkTime.localTime);
            using (NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get())
            {
                if (batchForChannelId.GetBatch(networkWriterPooled))
                {
                    Transport.active.OnServerDataReceived(this.connectionId, networkWriterPooled.ToArraySegment(), channelId);
                }
                else
                {
                    Debug.LogError("Local connection failed to make batch. This should never happen.");
                }
            }
        }

        internal void Update()
        {
            foreach (KeyValuePair<int, Batcher> keyValuePair in this.batches)
            {
                using (NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get())
                {
                    while (keyValuePair.Value.GetBatch(networkWriterPooled))
                    {
                        ArraySegment<byte> segment = networkWriterPooled.ToArraySegment();
                        if (ValidatePacketSize(segment, keyValuePair.Key))
                        {
                            SendToTransport(segment, keyValuePair.Key);
                            networkWriterPooled.Position = 0;
                        }
                    }
                }
            }

            while (this.queue.Count > 0)
            {
                NetworkWriterPooled networkWriterPooled = this.queue.Dequeue();
                ArraySegment<byte> message = networkWriterPooled.ToArraySegment();
                Batcher batchForChannelId = GetBatchForChannelId(0);
                batchForChannelId.AddMessage(message, NetworkTime.localTime);
                using (NetworkWriterPooled networkWriterPooled2 = NetworkWriterPool.Get())
                {
                    if (batchForChannelId.GetBatch(networkWriterPooled2))
                    {
                        // Server locally "sent" messages to handle by client (AI)
                        Transport.active.OnClientDataReceived(networkWriterPooled2.ToArraySegment(), 0);
                    }
                }
                NetworkWriterPool.Return(networkWriterPooled);
            }
        }

        internal void DisconnectInternal()
        {
            this.isReady = false;
        }

        public override void Disconnect()
        {
            this.connectionToClient.DisconnectInternal();
            DisconnectInternal();
        }

        internal bool IsAlive(float timeout)
        {
            return true;
        }

        internal LocalConnectionToClient connectionToClient;

        internal readonly Queue<NetworkWriterPooled> queue = new Queue<NetworkWriterPooled>();
    }
}
