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
        public RoomKindArea(IEnumerable<RoomKindVertex> vertices)
        {
            Vertices.AddRange(vertices);
        }

        public (RoomName, RoomShape, RoomZone) RoomKind { get; set; }
        public List<RoomKindVertex> Vertices { get; } = new();

        public Vector3 LocalCenterPosition => Vertices.Select(v => v.LocalPosition)
            .Aggregate(Vector3.zero, (a, u) => a + u) / Vertices.Count;

        public List<RoomKindArea> ConnectedRoomKindAreas { get; } = new();

        public Dictionary<FacilityRoom, Area> AreasOfRoom { get; } = new();
    }
}
