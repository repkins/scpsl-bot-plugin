using AdminToys;
using InventorySystem.Items.Coin;
using MapGeneration;
using MEC;
using Mirror;
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

        public Dictionary<(RoomName, RoomShape), List<NodeTemplate>> NodeTemplatesByRoom { get; } = new Dictionary<(RoomName, RoomShape), List<NodeTemplate>>();
        
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

        public NodeTemplate AddNode(Vector3 localPosition, (RoomName, RoomShape) roomNameShape)
        {
            if (!NodeTemplatesByRoom.TryGetValue(roomNameShape, out var roomNodeTemplates))
            {
                roomNodeTemplates = new List<NodeTemplate>();
                NodeTemplatesByRoom.Add(roomNameShape, roomNodeTemplates);
            }

            var newNodeTemplate = new NodeTemplate(localPosition)
            {
                Id = roomNodeTemplates.Count,
                RoomNameShape = roomNameShape,
            };

            roomNodeTemplates.Add(newNodeTemplate);

            foreach (var roomOfNameShape in NodesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape) == roomNameShape))
            {
                roomOfNameShape.Value.Add(new Node(newNodeTemplate, roomOfNameShape.Key));
            }

            return newNodeTemplate;
        }

        public void RemoveNode(NodeTemplate nodeTemplate, (RoomName, RoomShape) roomNameShape)
        {
            if (!NodeTemplatesByRoom.TryGetValue(roomNameShape, out var roomNodeTemplates))
            {
                Log.Warning($"No nodes at room {roomNameShape} to remove node from.");
                return;
            }

            roomNodeTemplates.Remove(nodeTemplate);

            foreach (var roomOfNameShape in NodesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape) == roomNameShape))
            {
                roomOfNameShape.Value.RemoveAll(node => node.Template == nodeTemplate);
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

                if (!NodeTemplatesByRoom.TryGetValue((roomName, roomShape), out var nodeTemplates))
                {
                    nodeTemplates = new List<NodeTemplate>();
                    NodeTemplatesByRoom.Add((roomName, roomShape), nodeTemplates);
                }
                else
                {
                    nodeTemplates.Clear();
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

                    var connectedNodesCount = binaryReader.ReadInt32();

                    var connectedNodes = new int[connectedNodesCount];
                    for (var k = 0; k < connectedNodesCount; k++)
                    {
                        connectedNodes[k] = binaryReader.ReadInt32();
                    }

                    AddNode(localPos, (roomName, roomShape));
                    
                    nodesConnections[j] = connectedNodes;
                }

                foreach (var (node, conns) in nodesConnections.Select((conns, nodeIndex) => (nodeTemplates[nodeIndex], conns)))
                {
                    node.ConnectedNodes.AddRange(conns.Select(connectedIndex => nodeTemplates[connectedIndex]));
                }
            }
        }

        public void WriteNodes(BinaryWriter binaryWriter)
        {
            byte version = 1;
            binaryWriter.Write(version);

            binaryWriter.Write(NodeTemplatesByRoom.Count);

            foreach (var roomNodes in NodeTemplatesByRoom)
            {
                var (roomName, roomShape) = roomNodes.Key;
                var nodes = roomNodes.Value;

                binaryWriter.Write(roomName.ToString());
                binaryWriter.Write(roomShape.ToString());
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
