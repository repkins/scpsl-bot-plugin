using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class Node
    {
        public int Id => RoomKindNode.Id;
        public Vector3 LocalPosition => RoomKindNode.LocalPosition;
        public float Radius => RoomKindNode.Radius;
        public IEnumerable<Node> ConnectedNodes => RoomKindNode.ConnectedNodes.Select(k => k.NodesOfRoom[Room]);

        public FacilityRoom Room { get; private set; }
        public RoomKindNode RoomKindNode { get; private set; }

        public Node(RoomKindNode roomKindNode, FacilityRoom room)
        {
            Room = room;
            RoomKindNode = roomKindNode;

            RoomKindNode.NodesOfRoom.Add(Room, this);
        }

        ~Node()
        {
            RoomKindNode.NodesOfRoom.Remove(Room);
        }
    }
}
