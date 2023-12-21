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
    internal class NavigationMesh
    {
        public static NavigationMesh Instance { get; } = new();

        public Dictionary<(RoomName, RoomShape, RoomZone), List<RoomKindArea>> AreasByRoomKind { get; } = new();
        public Dictionary<FacilityRoom, List<Area>> AreasByRoom { get; } = new();
        public Dictionary<FacilityRoom, List<int>> VerticesByRoom { get; } = new();
        public Dictionary<FacilityRoom, List<Vector3>> VertexLocalPositionsByRoom { get; } = new();

        public void Init()
        { }

        public Area FindNearestArea(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!room || !AreasByRoom.TryGetValue(room.ApiRoom, out var roomAreas))
            {
                return null;
            }

            var localPosition = room.transform.InverseTransformPoint(position);

            return roomAreas.Find(a => IsPointWithinArea(a, localPosition));
        }

        private bool IsPointWithinArea(Area area, Vector3 point)
        {
            var areaLocalVertices = area.RoomKindArea.LocalVertices;
            var vertexLocalPositionsByRoom = VertexLocalPositionsByRoom[area.Room];

            var areaVertexPositions = areaLocalVertices.Select(vIdx => vertexLocalPositionsByRoom[vIdx]).ToList();

            var edges = areaVertexPositions.Zip(areaVertexPositions.Skip(1), (p1, p2) => (p1, p2)).Append((p1: areaVertexPositions.Last(), p2: areaVertexPositions.First()));

            return edges.Select(e => (dirTo2: e.p2 - e.p1, dirToPoint: point - e.p1))
                .Select(d => (edgeNormal: Vector3.Cross(d.dirTo2, Vector3.up), d.dirToPoint))
                .Select(d2 => Vector3.Dot(d2.edgeNormal, d2.dirToPoint))
                .All(p => p >= 0f);
        }

        #region Private constructor
        private NavigationMesh()
        { }
        #endregion
    }
}
