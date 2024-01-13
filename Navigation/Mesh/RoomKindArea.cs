using MapGeneration;
using PluginAPI.Core.Zones;
using SCPSLBot.MapGeneration;
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
        public List<RoomKindVertex> Vertices { get; } = new();

        public Vector3 LocalCenterPosition => Vertices.Select(v => v.LocalPosition)
            .Aggregate(Vector3.zero, (a, u) => a + u) / Vertices.Count;

        public IEnumerable<RoomKindEdge> Edges => Vertices.Zip(Vertices.Skip(1), (v1, v2) => new RoomKindEdge(v1, v2))
            .Append(new RoomKindEdge(Vertices.Last(), Vertices.First()));

        public List<RoomKindArea> ConnectedRoomKindAreas { get; } = new();
        public List<RoomKindEdge> ConnectedRoomKindAreaEdges = new();

        public Dictionary<FacilityRoom, Area> AreasOfRoom { get; } = new();

        public RoomKindArea()
        { }

        public RoomKindArea(IEnumerable<RoomKindVertex> vertices, (RoomName, RoomShape, RoomZone) roomKind)
        {
            RoomKind = roomKind;
            Vertices.AddRange(vertices);
        }

        public override string ToString()
        {
            return $"#{NavigationMesh.Instance.AreasByRoomKind[RoomKind].IndexOf(this)} {RoomKind}";
        }
    }
}
