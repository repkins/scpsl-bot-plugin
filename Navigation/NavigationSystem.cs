using MapGeneration;
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

            //var nodesConnections = new (Node node, int[])[]
            //{
            //    (new Node(new Vector3(-22.50f, 0.96f, 0.00f)), new int[] { 1 }),
            //    (new Node(new Vector3(-17.71f, 0.96f, 0.00f)), new int[] { 0, 2 }),
            //    (new Node(new Vector3(-13.82f, 0.96f, 0.00f)), new int[] { 1, 3 }),
            //    (new Node(new Vector3(-10.02f, 0.96f, 0.00f)), new int[] {  }),
            //    (new Node(new Vector3(-6.27f, 0.96f, 0.00f)), new int[] {  }),
            //    (new Node(new Vector3(-2.35f, 0.96f, 0.00f)), new int[] {  }),
            //    (new Node(new Vector3(1.45f, 0.96f, 0.00f)), new int[] {  }),
            //    (new Node(new Vector3(5.20f, 0.96f, -0.00f)), new int[] {  }),
            //    (new Node(new Vector3(5.45f, 0.96f, -4.07f)), new int[] {  }),
            //    (new Node(new Vector3(5.06f, 0.96f, 4.01f)), new int[] {  }),
            //    (new Node(new Vector3(1.62f, 0.96f, -4.37f)), new int[] {  }),
            //    (new Node(new Vector3(1.27f, 0.96f, 4.20f)), new int[] {  }),
            //    (new Node(new Vector3(-2.12f, 0.96f, -4.18f)), new int[] {  }),
            //    (new Node(new Vector3(-2.62f, 0.96f, 4.35f)), new int[] {  }),
            //    (new Node(new Vector3(-5.87f, 0.96f, -4.43f)), new int[] {  }),
            //    (new Node(new Vector3(-6.42f, 0.96f, 4.37f)), new int[] {  }),
            //    (new Node(new Vector3(-9.73f, 0.96f, -4.29f)), new int[] {  }),
            //    (new Node(new Vector3(-10.21f, 0.96f, 4.38f)), new int[] {  }),
            //    (new Node(new Vector3(-13.46f, 0.96f, -4.30f)), new int[] {  }),
            //    (new Node(new Vector3(-14.06f, 0.96f, 4.29f)), new int[] {  }),
            //    (new Node(new Vector3(-17.39f, 0.96f, -4.58f)), new int[] {  }),
            //    (new Node(new Vector3(-17.88f, 0.96f, 4.45f)), new int[] {  }),
            //};

            //foreach (var ((node, connsIdxs), nodeIdx) in nodesConnections.Select((nodeConns, idx) => (nodeConns, idx)))
            //{
            //    node.Id = nodeIdx;
            //    node.RoomNameShape = (RoomName.LczClassDSpawn, RoomShape.Endroom);
            //    node.ConnectedNodes.AddRange(connsIdxs.Select(connIdx => nodesConnections[connIdx].node));
            //}

            //NavigationGraph.NodesByRoom.Add((RoomName.LczClassDSpawn, RoomShape.Endroom), nodesConnections.Select(nodeConn => nodeConn.node).ToList());
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
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

        #region Private constructor
        private NavigationSystem()
        { }
        #endregion
    }
}
