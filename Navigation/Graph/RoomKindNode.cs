using MapGeneration;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class RoomKindNode
    {
        public int Id { get; set; }
        public Vector3 LocalPosition { get; set; }
        public float Radius { get; set; }
        public (RoomName, RoomShape, RoomZone) RoomKind { get; set; }

        public List<RoomKindNode> ConnectedNodes { get; } = new List<RoomKindNode>();
        public Dictionary<FacilityRoom, Node> NodesOfRoom { get; } = new Dictionary<FacilityRoom, Node>();

        public RoomKindNode(Vector3 localPosition)
        {
            LocalPosition = localPosition;
        }
    }
}
