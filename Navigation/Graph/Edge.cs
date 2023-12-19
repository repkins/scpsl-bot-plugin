using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class Edge
    {
        public RoomKindEdge RoomKindEdge { get; }
        public FacilityRoom Room { get; }

        public Vector3 Position => Room.Transform.TransformPoint(RoomKindEdge.LocalPosition);
        public Vector3 Direction => Room.Transform.TransformDirection(RoomKindEdge.LocalDirection);
        public float Extents => RoomKindEdge.Extents;

        public Edge(RoomKindEdge roomKindEdge, FacilityRoom room)
        {
            RoomKindEdge = roomKindEdge;
            Room = room;
        }
    }
}
