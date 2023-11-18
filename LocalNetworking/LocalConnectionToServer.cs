using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SCPSLBot.LocalNetworking
{
    internal class LocalConnectionToServer : NetworkConnectionToServer
    {
        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
            if (segment.Count == 0)
            {
                Debug.LogError("LocalConnection.SendBytes cannot send zero bytes");
                return;
            }
            Batcher batchForChannelId = base.GetBatchForChannelId(channelId);
            batchForChannelId.AddMessage(segment, NetworkTime.localTime);
            using (NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get())
            {
                if (batchForChannelId.GetBatch(networkWriterPooled))
                {
                    Transport.active.OnServerDataReceived(this.connectionToClient.connectionId, networkWriterPooled.ToArraySegment(), channelId);
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
                        this.SendToTransport(segment, keyValuePair.Key);
                        networkWriterPooled.Position = 0;
                    }
                }
            }

            while (queue.Count > 0)
            {
                NetworkWriterPooled networkWriterPooled = queue.Dequeue();
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
            isReady = false;
        }

        public override void Disconnect()
        {
            connectionToClient.DisconnectInternal();
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
