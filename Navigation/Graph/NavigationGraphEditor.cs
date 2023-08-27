using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraphEditor
    {
        public static NavigationGraphEditor Instance { get; } = new NavigationGraphEditor();

        public NavigationGraph NavigationGraph { get; } = NavigationGraph.Instance;

        public bool IsEditing { get; set; }
        public Player PlayerEditing { get; set; }
        public Node LastEditingNode { get; set; }
        public Dictionary<Node, PrimitiveObjectToy> NodeVisuals { get; } = new Dictionary<Node, PrimitiveObjectToy>();
        public Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();
        public Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionOriginVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();

        public void Init()
        {
            Timing.RunCoroutine(RunNodeInfoVisuals());
            Timing.RunCoroutine(RunNodeVisuals());
        }
        public IEnumerator<float> RunNodeInfoVisuals()
        {
            while (true)
            {
                UpdateNodeInfoVisuals();

                yield return Timing.WaitForOneFrame;
            }
        }

        public void UpdateNodeInfoVisuals()
        {
            if (PlayerEditing != null)
            {
                var player = PlayerEditing;

                var nearestNode = NavigationGraph.FindNearestNode(player.Position);

                if (nearestNode != LastEditingNode)
                {
                    LastEditingNode = nearestNode;

                    if (nearestNode == null)
                    {
                        player.ClearBroadcasts();
                    }
                    else
                    {
                        var connectedIdsStr = string.Join(", ", nearestNode.ConnectedNodes.Select(c => $"#{c.Id}"));
                        var message = $"Node #{nearestNode.Id} in {nearestNode.RoomNameShape} connected to {connectedIdsStr}";
                        player.SendBroadcast(message, 60, shouldClearPrevious: true);
                    }
                }
            }
        }

        public IEnumerator<float> RunNodeVisuals()
        {
            while (true)
            {
                UpdateNodeVisuals();

                yield return Timing.WaitForOneFrame;
            }
        }

        public void UpdateNodeVisuals()
        {
            if (PlayerEditing != null)
            {
                var primPrefab = _primPrefab;

                foreach (var node in NavigationGraph.NodesByRoom.Values.SelectMany(l => l))
                {
                    var (roomName, roomShape) = node.RoomNameShape;
                    RoomIdUtils.TryFindRoom(roomName, FacilityZone.None, roomShape, out var room);

                    if (!NodeVisuals.TryGetValue(node, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        NetworkServer.Spawn(visual.gameObject);

                        visual.transform.position = room.transform.TransformPoint(node.LocalPosition);
                        visual.transform.localScale = Vector3.one * 0.25f;
                        visual.NetworkMaterialColor = Color.yellow;

                        NodeVisuals.Add(node, visual);
                    }

                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        if (NodeConnectionVisuals.TryGetValue((connectedNode, node), out var inConnectionVisual))
                        {
                            if (NodeConnectionOriginVisuals.TryGetValue((connectedNode, node), out var connectionOriginVisual))
                            {
                                inConnectionVisual.NetworkMaterialColor = Color.yellow;

                                NodeConnectionOriginVisuals.Remove((connectedNode, node));
                                NetworkServer.Destroy(connectionOriginVisual.gameObject);
                            }

                            continue;
                        }

                        if (!NodeConnectionVisuals.TryGetValue((node, connectedNode), out var outConnectionVisual))
                        {
                            outConnectionVisual = UnityEngine.Object.Instantiate(primPrefab);
                            outConnectionVisual.NetworkPrimitiveType = PrimitiveType.Cylinder;
                            outConnectionVisual.transform.position = room.transform.TransformPoint(Vector3.Lerp(node.LocalPosition, connectedNode.LocalPosition, 0.5f));
                            outConnectionVisual.transform.LookAt(room.transform.TransformPoint(connectedNode.LocalPosition));
                            outConnectionVisual.transform.Rotate(outConnectionVisual.transform.forward, 90f);
                            outConnectionVisual.transform.localScale = -Vector3.forward * 0.1f + -Vector3.right * 0.1f;
                            outConnectionVisual.transform.localScale += -Vector3.up * Vector3.Distance(node.LocalPosition, connectedNode.LocalPosition) * 0.5f;
                            outConnectionVisual.NetworkMaterialColor = Color.white;
                            NetworkServer.Spawn(outConnectionVisual.gameObject);

                            var connectionOriginVisual = UnityEngine.Object.Instantiate(primPrefab);
                            connectionOriginVisual.transform.position = outConnectionVisual.transform.position;
                            connectionOriginVisual.transform.position += outConnectionVisual.transform.up * (outConnectionVisual.transform.localScale.y + 0.125f);
                            connectionOriginVisual.transform.localScale = Vector3.one * 0.2f;
                            connectionOriginVisual.NetworkMaterialColor = Color.white;
                            NetworkServer.Spawn(connectionOriginVisual.gameObject);

                            NodeConnectionVisuals.Add((node, connectedNode), outConnectionVisual);
                            NodeConnectionOriginVisuals.Add((node, connectedNode), connectionOriginVisual);
                        }
                    }
                }
            }
            else
            {
                foreach (var nodeVisual in NodeVisuals.Values)
                {
                    NetworkServer.Destroy(nodeVisual.gameObject);
                }
                NodeVisuals.Clear();

                foreach (var connectionVisual in NodeConnectionVisuals.Values)
                {
                    NetworkServer.Destroy(connectionVisual.gameObject);
                }
                NodeConnectionVisuals.Clear();

                foreach (var connectionOriginVisual in NodeConnectionOriginVisuals.Values)
                {
                    NetworkServer.Destroy(connectionOriginVisual.gameObject);
                }
                NodeConnectionOriginVisuals.Clear();
            }
        }

        private PrimitiveObjectToy _primPrefab;

        #region Private constructor
        private NavigationGraphEditor()
        {
            _primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);
        }
        #endregion
    }
}
