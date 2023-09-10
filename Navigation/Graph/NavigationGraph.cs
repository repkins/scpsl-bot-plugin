using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Zones;
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

        public Dictionary<(RoomName, RoomShape, RoomZone), List<RoomKindNode>> NodesByRoomKind { get; } = new Dictionary<(RoomName, RoomShape, RoomZone), List<RoomKindNode>>();
        
        public Dictionary<FacilityRoom, List<Node>> NodesByRoom { get; } = new Dictionary<FacilityRoom, List<Node>>();

        public void Init()
        { }

        public Node FindNearestNode(Vector3 position, float radius = 1f)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!NodesByRoom.TryGetValue(room.ApiRoom, out var roomNodes))
            {
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

        public RoomKindNode AddNode(Vector3 localPosition, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!NodesByRoomKind.TryGetValue(roomKind, out var roomKindNodes))
            {
                roomKindNodes = new List<RoomKindNode>();
                NodesByRoomKind.Add(roomKind, roomKindNodes);
            }

            var newRoomKindNode = new RoomKindNode(localPosition)
            {
                Id = roomKindNodes.Count,
                RoomKind = roomKind,
            };

            roomKindNodes.Add(newRoomKindNode);

            foreach (var roomOfKind in NodesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                roomOfKind.Value.Add(new Node(newRoomKindNode, roomOfKind.Key));
            }

            return newRoomKindNode;
        }

        public void RemoveNode(RoomKindNode roomKindNode, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!NodesByRoomKind.TryGetValue(roomKind, out var roomKindNodes))
            {
                Log.Warning($"No nodes at room {roomKind} to remove node from.");
                return;
            }

            foreach (var connectedToRemovingNode in roomKindNode.ConnectedNodes)
            {
                connectedToRemovingNode.ConnectedNodes.Remove(roomKindNode);
            }

            roomKindNodes.Remove(roomKindNode);

            foreach (var roomOfKind in NodesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                var node = roomOfKind.Value.Find(n => n.RoomKindNode == roomKindNode);
                roomOfKind.Value.Remove(node);
            }
        }

        public void ReadNodes(BinaryReader binaryReader)
        {
            var version = binaryReader.ReadByte();

            var roomCount = binaryReader.ReadInt32();

            for (var i = 0; i < roomCount; i++)
            {
                Enum.TryParse<RoomName>(binaryReader.ReadString(), out var roomName);
                Enum.TryParse<RoomShape>(binaryReader.ReadString(), out var roomShape);

                RoomZone roomZone;
                if (version > 1)
                {
                    Enum.TryParse<RoomZone>(binaryReader.ReadString(), out roomZone);
                }
                else
                {
                    roomZone = RoomZone.LightContainment;
                }

                if (!NodesByRoomKind.TryGetValue((roomName, roomShape, roomZone), out var roomKindNodes))
                {
                    roomKindNodes = new List<RoomKindNode>();
                    NodesByRoomKind.Add((roomName, roomShape, roomZone), roomKindNodes);
                }
                else
                {
                    roomKindNodes.Clear();
                }

                var nodesCount = binaryReader.ReadInt32();
                var nodesConnections = new int[nodesCount][];

                for (var j = 0; j < nodesCount; j++)
                {
                    var localPos = new Vector3()
                    {
                        x = binaryReader.ReadSingle(),
                        y = binaryReader.ReadSingle(),
                        z = binaryReader.ReadSingle()
                    };

                    var radius = binaryReader.ReadSingle();

                    var newRoomKindNode = new RoomKindNode(localPos)
                    {
                        Id = roomKindNodes.Count,
                        RoomKind = (roomName, roomShape, roomZone),
                    };
                    roomKindNodes.Add(newRoomKindNode);

                    var connectedNodesCount = binaryReader.ReadInt32();

                    var connectedNodes = new int[connectedNodesCount];
                    for (var k = 0; k < connectedNodesCount; k++)
                    {
                        connectedNodes[k] = binaryReader.ReadInt32();
                    }
                    
                    nodesConnections[j] = connectedNodes;
                }

                foreach (var (node, conns) in nodesConnections.Select((conns, nodeIndex) => (roomKindNodes[nodeIndex], conns)))
                {
                    node.ConnectedNodes.AddRange(conns.Select(connectedIndex => roomKindNodes[connectedIndex]));
                }
            }
        }

        public void WriteNodes(BinaryWriter binaryWriter)
        {
            byte version = 2;
            binaryWriter.Write(version);

            binaryWriter.Write(NodesByRoomKind.Count);

            foreach (var roomNodes in NodesByRoomKind)
            {
                var (roomName, roomShape, roomZone) = roomNodes.Key;
                var nodes = roomNodes.Value;

                binaryWriter.Write(roomName.ToString());
                binaryWriter.Write(roomShape.ToString());
                binaryWriter.Write(roomZone.ToString());
                binaryWriter.Write(nodes.Count);

                foreach (var node in nodes)
                {
                    binaryWriter.Write(node.LocalPosition.x);
                    binaryWriter.Write(node.LocalPosition.y);
                    binaryWriter.Write(node.LocalPosition.z);

                    binaryWriter.Write(node.Radius);

                    binaryWriter.Write(node.ConnectedNodes.Count);

                    foreach (var connIdx in node.ConnectedNodes.Select(connNode => connNode.Id))
                    {
                        binaryWriter.Write(connIdx);
                    }
                }
            }
        }

        #region Private constructor
        private NavigationGraph()
        { }
        #endregion
    }
}
