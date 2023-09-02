using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraphVisuals
    {
        public Player EnabledVisualsForPlayer { get; set; }

        public NodeTemplate NearestNodeTemplate { get; set; }
        public NodeTemplate FacingNodeTemplate { get; set; }

        private Dictionary<Node, PrimitiveObjectToy> NodeVisuals { get; } = new Dictionary<Node, PrimitiveObjectToy>();
        private Dictionary<(Node From, Node To), PrimitiveObjectToy> NodeConnectionVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();
        private Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionOriginVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();

        private NavigationGraph NavigationGraph { get; } = NavigationGraph.Instance;

        private NodeTemplate LastNearestNodeTemplate { get; set; }
        private NodeTemplate LastFacingNodeTemplate { get; set; }

        private string[] NodeVisualsMessages { get; } = new string[2];

        public void UpdateNodeInfoVisuals()
        {
            if (EnabledVisualsForPlayer != null)
            {
                var nearestNode = NearestNodeTemplate;

                if (nearestNode != LastNearestNodeTemplate)
                {
                    LastNearestNodeTemplate = nearestNode;

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

                if (FacingNodeTemplate != LastFacingNodeTemplate)
                {
                    LastFacingNodeTemplate = FacingNodeTemplate;

                    if (FacingNodeTemplate != null)
                    {
                        NodeVisualsMessages[1] = $"Facing node #{FacingNodeTemplate.Id} in {FacingNodeTemplate.RoomNameShape}";
                    }
                    else
                    {
                        NodeVisualsMessages[1] = null;
                    }
                }

                var messageLinesToSend = NodeVisualsMessages.Where(m => m != null);
                if (messageLinesToSend.Any())
                {
                    EnabledVisualsForPlayer.SendBroadcast(string.Join("\n", messageLinesToSend), 60, shouldClearPrevious: true);
                }
                else
                {
                    EnabledVisualsForPlayer.ClearBroadcasts();
                }
            }
        }

        public void UpdateNodeVisuals()
        {
            if (EnabledVisualsForPlayer != null)
            {
                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var nodeVisual in NodeVisuals.ToArray())
                {
                    if (!NavigationGraph.NodesByRoom.Values.Any(l => l.Contains(nodeVisual.Key)))
                    {
                        NetworkServer.Destroy(nodeVisual.Value.gameObject);
                        NodeVisuals.Remove(nodeVisual.Key);
                    }
                }

                foreach (var ((from, to), visual) in NodeConnectionVisuals.Select(p => (p.Key, p.Value)).ToArray())
                {
                    var isAnyNodeRemoved = !NodeVisuals.ContainsKey(from) || !NodeVisuals.ContainsKey(to);

                    if (isAnyNodeRemoved || (!from.ConnectedNodes.Contains(to) && !to.ConnectedNodes.Contains(from)))
                    {
                        NetworkServer.Destroy(visual.gameObject);
                        NodeConnectionVisuals.Remove((from, to));
                    }

                    if ((isAnyNodeRemoved || !from.ConnectedNodes.Contains(to)) && NodeConnectionOriginVisuals.TryGetValue((from, to), out var originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        NodeConnectionOriginVisuals.Remove((from, to));
                    }

                    if ((isAnyNodeRemoved || !to.ConnectedNodes.Contains(from)) && NodeConnectionOriginVisuals.TryGetValue((to, from), out originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        NodeConnectionOriginVisuals.Remove((to, from));
                    }
                }

                foreach (var node in NavigationGraph.NodesByRoom.Values.SelectMany(l => l))
                {
                    var room = node.Room.Identifier;

                    if (!NodeVisuals.TryGetValue(node, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        NetworkServer.Spawn(visual.gameObject);

                        visual.transform.position = room.transform.TransformPoint(node.LocalPosition);
                        visual.transform.localScale = -Vector3.one * 0.25f;

                        NodeVisuals.Add(node, visual);
                    }

                    visual.NetworkMaterialColor = (node.Template == FacingNodeTemplate) ? Color.green : Color.yellow;

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
    }
}
