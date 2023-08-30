using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraph
    {
        public static NavigationGraph Instance { get; } = new NavigationGraph();

        public Dictionary<(RoomName, RoomShape), List<Node>> NodesByRoom { get; } = new Dictionary<(RoomName, RoomShape), List<Node>>()
        {
            {
                (RoomName.LczClassDSpawn, RoomShape.Endroom), new List<Node>
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
        { }

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

        public Node AddNode(Vector3 localPosition, (RoomName, RoomShape) roomNameShape, int[] connectedNodesIndices = null)
        {
            if (!NodesByRoom.TryGetValue(roomNameShape, out var roomNodes))
            {
                roomNodes = new List<Node>();
                NodesByRoom.Add(roomNameShape, roomNodes);
            }

            var newNode = new Node(localPosition, connectedNodesIndices)
            {
                Id = roomNodes.Count,
                RoomNameShape = roomNameShape,
            };

            roomNodes.Add(newNode);

            return newNode;
        }

        public void InitNodeGraph()
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
