using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation
{
    internal class NavigationSystem
    {
        public static NavigationSystem Instance { get; } = new NavigationSystem();

        public string BaseDir { get; set; }

        private NavigationGraph NavigationGraph { get; } = NavigationGraph.Instance;

        public void Init()
        {
            EventManager.RegisterEvents(this);

            LoadNodes();
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            Log.Info($"Initializing nodes from room kind nodes.");

            InitRoomNodes();

            // 
            // Connect door waypoints
        }

        public void LoadNodes()
        {
            var fileName = "navgraph.slngf";
            var path = Path.Combine(BaseDir, fileName);
            using (var fileStream = File.OpenRead(path))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                NavigationGraph.ReadNodes(binaryReader);
            }
        }

        public void SaveNodes()
        {
            var fileName = "navgraph.slngf";
            var path = Path.Combine(BaseDir, fileName);
            using (var fileStream = File.OpenWrite(path))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                NavigationGraph.WriteNodes(binaryWriter);
            }
        }

        public void InitRoomNodes()
        {
            foreach (var room in Facility.Rooms)
            {
                var nodes = new List<Node>();
                NavigationGraph.NodesByRoom.Add(room, nodes);

                if (!NavigationGraph.NodesByRoomKind.TryGetValue((room.Identifier.Name, room.Identifier.Shape, (RoomZone)room.Identifier.Zone), out var roomKindNodes))
                {
                    continue;
                }

                nodes.AddRange(roomKindNodes.Select(k => new Node(k, room)));
            }
        }

        public void ResetNodes()
        {
            NavigationGraph.NodesByRoom.Clear();
        }

        #region Private constructor
        private NavigationSystem()
        { }
        #endregion
    }
}
