using MapGeneration;
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
    internal class NavigationGraphManager
    {
        public static NavigationGraphManager Instance { get; } = new NavigationGraphManager();

        private NavigationGraph NavigationGraph { get; } = NavigationGraph.Instance;

        public void Init()
        {
            EventManager.RegisterEvents(this);

            NavigationGraph.InitNodeGraph();
            //LoadNodes();
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            // Connect door waypoints
        }

        public void LoadNodes()
        {
            var fileName = "navgraph.slngf";
            var path = Path.Combine(Assembly.GetExecutingAssembly().Location, "SCPSLBot", fileName);
            using (var fileStream = File.OpenRead(path))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var version = binaryReader.ReadByte();

                var roomCount = binaryReader.ReadInt32();

                for (var i = 0; i < roomCount; i++)
                {
                    Enum.TryParse<RoomName>(binaryReader.ReadString(), out var roomName);
                    Enum.TryParse<RoomShape>(binaryReader.ReadString(), out var roomShape);

                    if (!NavigationGraph.NodesByRoom.TryGetValue((roomName, roomShape), out var nodes))
                    {
                        nodes = new List<Node>();
                        NavigationGraph.NodesByRoom.Add((roomName, roomShape), nodes);
                    }
                    else
                    {
                        nodes.Clear();
                    }

                    var nodesCount = binaryReader.ReadInt32();

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

                        NavigationGraph.AddNode(localPos, (roomName, roomShape), connectedNodes);
                    }
                }
            }

            NavigationGraph.InitNodeGraph();
        }

        public void WriteNodes()
        {
            var fileName = "navgraph.slngf";
            var path = Path.Combine(Assembly.GetExecutingAssembly().Location, "SCPSLBot", fileName);
            using (var fileStream = File.OpenWrite(path))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                byte version = 1;
                binaryWriter.Write(version);

                var roomsWithNodes = NavigationGraph.NodesByRoom;

                binaryWriter.Write(roomsWithNodes.Count);

                foreach (var roomNodes in roomsWithNodes)
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

                        foreach (var i in node.ConnectedNodes.Select((_, i) => i))
                        {
                            binaryWriter.Write(i);
                        }
                    }
                }
            }
        }

        #region Private constructor
        private NavigationGraphManager()
        { }
        #endregion
    }
}
