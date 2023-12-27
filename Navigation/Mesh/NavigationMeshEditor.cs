using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class NavigationMeshEditor
    {
        public static NavigationMeshEditor Instance { get; } = new();

        public bool IsEditing { get; set; }
        public Player PlayerEditing { get; set; }

        private NavigationMesh NavigationMesh { get; } = NavigationMesh.Instance;
        private NavigationMeshVisuals Visuals { get; } = new();

        private Player LastPlayerEditing { get; set; }

        private Area CachedArea { get; set; }
        private Area TracingEndingArea { get; set; }

        public RoomKindArea FindClosestAreaFacingAt((RoomName, RoomShape, RoomZone) roomKind, Vector3 localPosition, Vector3 localDirection)
        {
            if (!NavigationMesh.Instance.AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                return null;
            }

            var targetArea = roomKindAreas
                .Select(a => (n: a, d: Vector3.SqrMagnitude(a.LocalCenterPosition - localPosition)))
                .Where(t => t.d < 50f && t.d > 1f)
                .OrderBy(t => t.d)
                .Select(t => t.n)
                .FirstOrDefault(a => Vector3.Dot(Vector3.Normalize(a.LocalCenterPosition - localPosition), localDirection) > 0.999848f);

            return targetArea;
        }

        public RoomKindArea MakeArea(Vector3 position, IEnumerable<Vector3> vertices)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var newArea = NavigationMesh.AddArea(vertices, roomKind);

            Log.Info($"Area #{NavigationMesh.AreasByRoomKind[roomKind].IndexOf(newArea)} at local center position {newArea.LocalCenterPosition} added under room {roomKind}.");

            return newArea;
        }

        public bool DissolveArea(Vector3 position)
        {
            var area = NavigationMesh.GetAreaWithin(position);
            if (area == null)
            {
                Log.Warning($"No area found within to remove.");

                return false;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            NavigationMesh.RemoveArea(area.RoomKindArea, roomKind);

            Log.Info($"Area at local center position {area.CenterPosition} removed under room {roomKind}.");

            return true;
        }
    }
}
