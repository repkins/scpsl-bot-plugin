using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Events;
using RemoteAdmin;
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
        public Dictionary<Node, PrimitiveObjectToy> NodeVisuals { get; } = new Dictionary<Node, PrimitiveObjectToy>();
        public Dictionary<(Node From, Node To), PrimitiveObjectToy> NodeConnectionVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();
        public Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionOriginVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();

        public Node FacingNode { get; private set; }

        public Node LastEditingNode { get; set; }
        public Node LastFacingNode { get; private set; }
        public string[] NodeVisualsMessages { get; } = new string[2];

        public void Init()
        {
            Timing.RunCoroutine(RunFacingNodeUpdates());
            Timing.RunCoroutine(RunNodeInfoVisuals());
            Timing.RunCoroutine(RunNodeVisuals());
        }

        public Node FindClosestNodeFacingAt((RoomName, RoomShape) roomNameShape, Vector3 localPosition, Vector3 localDirection)
        {
            var targetNode = NavigationGraph.Instance.NodesByRoom[roomNameShape]
                .Select(n => (n, d: Vector3.SqrMagnitude(n.LocalPosition - localPosition)))
                .Where(t => t.d < 50f && t.d > 1f)
                .OrderBy(t => t.d)
                .Select(t => t.n)
                .FirstOrDefault(n => Vector3.Dot(Vector3.Normalize(n.LocalPosition - localPosition), localDirection) > 0.999848f);

            return targetNode;
        }

        public IEnumerator<float> RunFacingNodeUpdates()
        {
            while (true)
            {
                UpdateFacingNode();

                yield return Timing.WaitForOneFrame;
            }
        }

        public void UpdateFacingNode()
        {
            if (PlayerEditing != null)
            {
                var room = RoomIdUtils.RoomAtPositionRaycasts(PlayerEditing.Position);

                var localPosition = room.transform.InverseTransformPoint(PlayerEditing.Camera.position);
                var localForward = room.transform.InverseTransformDirection(PlayerEditing.Camera.forward);

                FacingNode = FindClosestNodeFacingAt((room.Name, room.Shape), localPosition, localForward);
            }
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

                    if (nearestNode != null)
                    {
                        var connectedIdsStr = string.Join(", ", nearestNode.ConnectedNodes.Select(c => $"#{c.Id}"));
                        NodeVisualsMessages[0] = $"Node #{nearestNode.Id} in {nearestNode.RoomNameShape} connected to {connectedIdsStr}";
                    }
                    else
                    {
                        NodeVisualsMessages[0] = null;
                    }
                }

                if (FacingNode != LastFacingNode)
                {
                    LastFacingNode = FacingNode;

                    if (FacingNode != null)
                    {
                        NodeVisualsMessages[1] = $"Facing node #{FacingNode.Id} in {FacingNode.RoomNameShape}";
                    }
                    else
                    {
                        NodeVisualsMessages[1] = null;
                    }
                }

                var messageLinesToSend = NodeVisualsMessages.Where(m => m != null);
                if (messageLinesToSend.Any())
                {
                    player.SendBroadcast(string.Join("\n", messageLinesToSend), 60, shouldClearPrevious: true);
                }
                else
                {
                    player.ClearBroadcasts();
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
                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var ((from, to), visual) in NodeConnectionVisuals.Select(p => (p.Key, p.Value)).ToArray())
                {
                    if (!from.ConnectedNodes.Contains(to) && !to.ConnectedNodes.Contains(from))
                    {
                        NetworkServer.Destroy(visual.gameObject);
                        NodeConnectionVisuals.Remove((from, to));
                    }

                    if (!from.ConnectedNodes.Contains(to) && NodeConnectionOriginVisuals.TryGetValue((from, to), out var originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        NodeConnectionOriginVisuals.Remove((from, to));
                    }

                    if (!to.ConnectedNodes.Contains(from) && NodeConnectionOriginVisuals.TryGetValue((to, from), out originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        NodeConnectionOriginVisuals.Remove((to, from));
                    }
                }

                foreach (var node in NavigationGraph.NodesByRoom.Values.SelectMany(l => l))
                {
                    var (roomName, roomShape) = node.RoomNameShape;
                    RoomIdUtils.TryFindRoom(roomName, FacilityZone.None, roomShape, out var room);

                    if (!NodeVisuals.TryGetValue(node, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        NetworkServer.Spawn(visual.gameObject);

                        visual.transform.position = room.transform.TransformPoint(node.LocalPosition);
                        visual.transform.localScale = -Vector3.one * 0.25f;

                        NodeVisuals.Add(node, visual);
                    }

                    visual.NetworkMaterialColor = (node == FacingNode) ? Color.green : Color.yellow;

                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        if (!NodeConnectionVisuals.TryGetValue((node, connectedNode), out var outConnectionVisual))
                        {
                            if (!NodeConnectionVisuals.TryGetValue((connectedNode, node), out var inConnectionVisual))
                            {
                                outConnectionVisual = UnityEngine.Object.Instantiate(primPrefab);
                                outConnectionVisual.NetworkPrimitiveType = PrimitiveType.Cylinder;
                                outConnectionVisual.transform.position = room.transform.TransformPoint(Vector3.Lerp(node.LocalPosition, connectedNode.LocalPosition, 0.5f));
                                outConnectionVisual.transform.LookAt(room.transform.TransformPoint(connectedNode.LocalPosition));
                                outConnectionVisual.transform.RotateAround(outConnectionVisual.transform.position, outConnectionVisual.transform.right, 90f);
                                outConnectionVisual.transform.localScale = -Vector3.forward * 0.1f + -Vector3.right * 0.1f;
                                outConnectionVisual.transform.localScale += -Vector3.up * Vector3.Distance(node.LocalPosition, connectedNode.LocalPosition) * 0.5f;
                                NetworkServer.Spawn(outConnectionVisual.gameObject);

                                NodeConnectionVisuals.Add((node, connectedNode), outConnectionVisual);
                            }
                            else
                            {
                                outConnectionVisual = inConnectionVisual;
                            }
                        }

                        if (!NodeConnectionOriginVisuals.TryGetValue((node, connectedNode), out var outConnectionOriginVisual))
                        {
                            outConnectionOriginVisual = UnityEngine.Object.Instantiate(primPrefab);
                            outConnectionOriginVisual.transform.position = room.transform.TransformPoint(node.LocalPosition);
                            outConnectionOriginVisual.transform.position += room.transform.TransformDirection(Vector3.Normalize(connectedNode.LocalPosition - node.LocalPosition)) * 0.125f;
                            outConnectionOriginVisual.transform.localScale = -Vector3.one * 0.2f;
                            NetworkServer.Spawn(outConnectionOriginVisual.gameObject);

                            NodeConnectionOriginVisuals.Add((node, connectedNode), outConnectionOriginVisual);
                        }

                        if (NodeConnectionOriginVisuals.TryGetValue((connectedNode, node), out var inConnectionOriginVisual))
                        {
                            outConnectionVisual.NetworkMaterialColor = Color.yellow;
                            outConnectionOriginVisual.NetworkMaterialColor = Color.yellow;
                            inConnectionOriginVisual.NetworkMaterialColor = Color.yellow;
                        }
                        else
                        {
                            outConnectionVisual.NetworkMaterialColor = Color.white;
                            outConnectionOriginVisual.NetworkMaterialColor = Color.white;
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

        #region Private constructor
        private NavigationGraphEditor()
        { }
        #endregion
    }
}
