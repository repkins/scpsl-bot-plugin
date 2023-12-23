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

        public void Init()
        { }

        public Area GetAreaWithin(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!room || !AreasByRoom.TryGetValue(room.ApiRoom, out var roomAreas))
            {
                return null;
            }

            var localPosition = room.transform.InverseTransformPoint(position);

            return roomAreas.Find(a => IsPointWithinArea(a, localPosition));
        }

        private bool IsPointWithinArea(Area area, Vector3 pointLocalPosition)
        {
            var areaRoomKindVertices = area.RoomKindArea.Vertices;

            var areaRoomKindEdges = areaRoomKindVertices.Zip(areaRoomKindVertices.Skip(1), (v1, v2) => (v1, v2))
                .Append((v1: areaRoomKindVertices.Last(), v2: areaRoomKindVertices.First()));
            
            return areaRoomKindEdges.Select(e => (
                    dirTo2: e.v2.Position - e.v1.Position, 
                    dirToPoint: pointLocalPosition - e.v1.Position))
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
