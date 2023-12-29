using PluginAPI.Core;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class RoomVertex
    {
        public RoomKindVertex RoomKindVertex { get; }
        public FacilityRoom Room { get; }

        public Vector3 Position => Room.Transform.TransformPoint(RoomKindVertex.LocalPosition);

        public RoomVertex(RoomKindVertex roomKindVertex, FacilityRoom room)
        {
            RoomKindVertex = roomKindVertex;
            Room = room;
        }
    }
}
