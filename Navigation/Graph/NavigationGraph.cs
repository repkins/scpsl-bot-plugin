using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraph
    {
        public static NavigationGraph Instance { get; } = new NavigationGraph();

        public Dictionary<(RoomName, RoomShape), Node[]> NodesByRoom { get; } = new Dictionary<(RoomName, RoomShape), Node[]>()
        {
            {
                (RoomName.LczClassDSpawn, RoomShape.Endroom), new Node[]
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

        public List<Node> Nodes { get; } = new List<Node>();

        public void Init()
        {
            InitNodeGraph();

            EventManager.RegisterEvents(this);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            // Connect door waypoints
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

            var newNode = new Node(room.transform.InverseTransformPoint(position), new int[] { })
            {
                Id = Nodes.Count,
                RoomNameShape = (room.Name, room.Shape),
            };

            Nodes.Add(newNode);

            Log.Info($"Node #{newNode.Id} at local position {newNode.LocalPosition} added under room {(room.Name, room.Shape)}.");

            return newNode;
        }

        private void InitNodeGraph()
        {
            foreach (var (node, i) in NodesByRoom.Values.SelectMany(nr => nr).Select((n, i) => (n, i)))
            {
                node.Id = i;
                Nodes.Add(node);
            }

            foreach (var roomNodes in NodesByRoom)
            {
                foreach (var node in roomNodes.Value.Select(n => n))
                {
                    node.RoomNameShape = roomNodes.Key;
                    node.ConnectedNodes.AddRange(node.ConnectedNodeIndices.Select(i => Nodes[i]));
                }
            }
        }

        #region Private constructor
        private NavigationGraph()
        { }
        #endregion
    }
}
