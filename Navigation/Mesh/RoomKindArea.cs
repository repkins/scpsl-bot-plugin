using MapGeneration;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class RoomKindArea
    {
        public (RoomName, RoomShape, RoomZone) RoomKind { get; set; }
        public List<Vertex> Vertices { get; } = new();

        public List<RoomKindArea> ConnectedRoomKindAreas { get; } = new();

        public Dictionary<FacilityRoom, Area> AreasOfRoom { get; } = new();
    }
}
