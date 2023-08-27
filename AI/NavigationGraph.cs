using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using SCPSLBot.AI.NavigationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI
{
    internal class NavigationGraph
    {
        public static NavigationGraph Instance { get; } = new NavigationGraph();

        public bool IsEditing { get; set; }
        public Player PlayerEditing { get; set; }
        public Node LastEditingNode { get; set; }
        public Dictionary<Node, PrimitiveObjectToy> NodeVisuals { get; } = new Dictionary<Node, PrimitiveObjectToy>();
        public Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();
        public Dictionary<(Node, Node), PrimitiveObjectToy> NodeConnectionOriginVisuals { get; } = new Dictionary<(Node, Node), PrimitiveObjectToy>();

        public static Dictionary<(RoomName, RoomShape), List<Node>> NodesByRoom { get; } = new Dictionary<(RoomName, RoomShape), List<Node>>()
        {
            { 
                (RoomName.LczClassDSpawn, RoomShape.Endroom), new List<Node>()
                {
                    new Node(new Vector3(-22.50f, 0.96f, 0.00f), new int[] { 1 }),
                    new Node(new Vector3(-17.71f, 0.96f, 0.00f), new int[] { 0, 2 }),
                    new Node(new Vector3(-13.82f, 0.96f, 0.00f), new int[] { 1, 3 }),
                    new Node(new Vector3(-10.02f, 0.96f, 0.00f), new int[] {  }),
                    new Node(new Vector3(-6.27f, 0.96f, 0.00f), new int[] {  }),
                    new Node(new Vector3(-2.35f, 0.96f, 0.00f), new int[] {  }),
                    new Node(new Vector3(1.45f, 0.96f, 0.00f), new int[] {  }),
                    new Node(new Vector3(5.20f, 0.96f, -0.00f), new int[] {  }),
                    new Node(new Vector3(5.45f, 0.96f, -4.07f), new int[] {  }),
                    new Node(new Vector3(5.06f, 0.96f, 4.01f), new int[] {  }),
                    new Node(new Vector3(1.62f, 0.96f, -4.37f), new int[] {  }),
                    new Node(new Vector3(1.27f, 0.96f, 4.20f), new int[] {  }),
                    new Node(new Vector3(-2.12f, 0.96f, -4.18f), new int[] {  }),
                    new Node(new Vector3(-2.62f, 0.96f, 4.35f), new int[] {  }),
                    new Node(new Vector3(-5.87f, 0.96f, -4.43f), new int[] {  }),
                    new Node(new Vector3(-6.42f, 0.96f, 4.37f), new int[] {  }),
                    new Node(new Vector3(-9.73f, 0.96f, -4.29f), new int[] {  }),
                    new Node(new Vector3(-10.21f, 0.96f, 4.38f), new int[] {  }),
                    new Node(new Vector3(-13.46f, 0.96f, -4.30f), new int[] {  }),
                    new Node(new Vector3(-14.06f, 0.96f, 4.29f), new int[] {  }),
                    new Node(new Vector3(-17.39f, 0.96f, -4.58f), new int[] {  }),
                    new Node(new Vector3(-17.88f, 0.96f, 4.45f), new int[] {  }),
                }
            }
        };

        public void Init()
        {
            InitNodeGraph();

            EventManager.RegisterEvents(this);

            Timing.RunCoroutine(RunNodeInfoVisuals());
            Timing.RunCoroutine(RunNodeVisuals());
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            // Connect door waypoints
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

                var nearestNode = FindNearestNode(player.Position);

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
                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var node in NodesByRoom.Values.SelectMany(l => l))
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
                            inConnectionVisual.NetworkMaterialColor = Color.yellow;

                            if (NodeConnectionOriginVisuals.TryGetValue((connectedNode, node), out var connectionOriginVisual))
                            {
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

        public Node FindNearestNode(Vector3 position, float radius = 1f)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            
            if (!NodesByRoom.TryGetValue((room.Name, room.Shape), out var roomNodes))
            {
                Log.Warning($"Can't find nodes at room {(room.Name, room.Shape)}.");
                return null;
            }

            var radiusSqr = Mathf.Pow(radius, 2);
            var localPosition = room.transform.InverseTransformPoint(position);

            var nodesWithinRadius = roomNodes.Select(node => (node, distSqr: Vector3.SqrMagnitude(node.LocalPosition - localPosition)))
                .Where(t => t.distSqr < radiusSqr);

            if (!nodesWithinRadius.Any())
            {
                return null;
            }

            return nodesWithinRadius
                .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                .node;
        }

        public Node AddNode(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!NodesByRoom.TryGetValue((room.Name, room.Shape), out var roomNodes))
            {
                roomNodes = new List<Node>();
                NodesByRoom.Add((room.Name, room.Shape), roomNodes);
            }

            var newNode = new Node(room.transform.InverseTransformPoint(position), new int[] { } )
            {
                Id = roomNodes.Count,
                RoomNameShape = (room.Name, room.Shape),
            };

            roomNodes.Add(newNode);

            Log.Info($"Node #{newNode.Id} at local position {newNode.LocalPosition} added under room {(room.Name, room.Shape)}.");

            return newNode;
        }

        private void InitNodeGraph()
        {
            foreach (var roomNodes in NodesByRoom)
            {
                foreach (var (node, i) in roomNodes.Value.Select((n, i) => (n, i)))
                {
                    node.Id = i;
                    node.RoomNameShape = roomNodes.Key;
                    node.ConnectedNodes.AddRange(node.ConnectedNodeIndices.Select(connectedIndex => roomNodes.Value[connectedIndex]));
                }
            }
        }

        #region Private constructor
        private NavigationGraph()
        { }
        #endregion
    }
}
