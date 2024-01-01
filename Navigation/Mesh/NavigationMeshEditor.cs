using MapGeneration;
using MEC;
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

        private List<RoomKindVertex> SeletedVertices { get; } = new();

        public void Init()
        {
            Visuals.SelectedVertices = SeletedVertices;

            Timing.RunCoroutine(RunEachFrame(UpdateEditing));
            Timing.RunCoroutine(RunEachFrame(UpdateNearestVertex));
            Timing.RunCoroutine(RunEachFrame(UpdateFacingVertex));
            Timing.RunCoroutine(RunEachFrame(UpdateNearestArea));
            Timing.RunCoroutine(RunEachFrame(UpdateFacingArea));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateVertexInfoVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateAreaInfoVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateAreaVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateVertexVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateEdgeVisuals));
        }

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

        public RoomKindVertex CreateVertex(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var localPosition = room.transform.InverseTransformPoint(position);

            var newVertex = NavigationMesh.AddVertex(localPosition, roomKind);

            Log.Info($"Vertex #{NavigationMesh.VerticesByRoomKind[roomKind].IndexOf(newVertex)} at local position {newVertex.LocalPosition} added under room {roomKind}.");

            return newVertex;
        }

        public bool DeleteVertex(Vector3 position)
        {
            var vertex = NavigationMesh.GetNearbyVertex(position);
            if (vertex == null)
            {
                Log.Warning($"No vertex found nearby to remove.");

                return false;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            NavigationMesh.DeleteVertex(vertex.RoomKindVertex);

            Log.Info($"Vertex at local position {vertex.RoomKindVertex.LocalPosition} removed under room {roomKind}.");

            return true;
        }

        public bool AddVertexToSelection(Vector3 position)
        {
            var vertex = NavigationMesh.GetNearbyVertex(position);
            if (vertex == null)
            {
                Log.Warning($"No vertex found nearby for selection.");
                return false;
            }

            SeletedVertices.Add(vertex.RoomKindVertex);

            Log.Info($"Vertex at local position {vertex.RoomKindVertex.LocalPosition} added to selection under room {vertex.RoomKindVertex.RoomKind}.");

            return true;
        }

        public bool RemoveVertexFromSelection(Vector3 position)
        {
            var vertex = NavigationMesh.GetNearbyVertex(position);
            if (vertex == null)
            {
                Log.Warning($"No vertex found nearby to remove from selection.");
                return false;
            }

            SeletedVertices.Remove(vertex.RoomKindVertex);

            Log.Info($"Vertex at local position {vertex.RoomKindVertex.LocalPosition} removed from selection under room {vertex.RoomKindVertex.RoomKind}.");

            return true;
        }

        public void ClearVertexSelection()
        {
            SeletedVertices.Clear();
        }

        public RoomKindArea MakeArea(Vector3 position)
        {
            if (SeletedVertices.Count < 3)
            {
                Log.Warning($"Not enough vertices (min 3) selected.");
                return null;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var newArea = NavigationMesh.AddArea(SeletedVertices, roomKind);

            Log.Info($"Area #{NavigationMesh.AreasByRoomKind[roomKind].IndexOf(newArea)} at local center position {newArea.LocalCenterPosition} added under room {roomKind}.");

            SeletedVertices.Clear();

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

            NavigationMesh.RemoveArea(area.RoomKindArea);

            Log.Info($"Area at local center position {area.CenterPosition} removed under room {roomKind}.");

            return true;
        }

        public bool CacheArea(Vector3 position)
        {
            CachedArea = NavigationMesh.GetAreaWithin(position);

            return CachedArea != null;
        }

        public bool TracePath(Vector3 position)
        {
            if (CachedArea == null)
            {
                return false;
            }

            var targetArea = NavigationMesh.GetAreaWithin(position);
            if (targetArea == null)
            {
                return false;
            }

            var path = NavigationMesh.GetShortestPath(CachedArea, targetArea);
            if (path.Count == 0)
            {
                Log.Warning($"No path found.");
            }

            Visuals.Path.Clear();
            Visuals.Path.AddRange(path);

            return true;
        }

        private void UpdateEditing()
        {
            if (PlayerEditing != LastPlayerEditing)
            {
                LastPlayerEditing = PlayerEditing;

                Visuals.PlayerEnabledVisualsFor = PlayerEditing;
            }
        }

        private void UpdateNearestVertex()
        {
            if (PlayerEditing != null)
            {
                Visuals.NearestVertex = NavigationMesh.GetNearbyVertex(PlayerEditing.Camera.position)?.RoomKindVertex;
            }
        }

        private void UpdateFacingVertex()
        {
            if (PlayerEditing != null)
            {
                Visuals.FacingVertex = NavigationMesh.GetNearbyVertex(PlayerEditing.Camera.position)?.RoomKindVertex;
            }
        }

        private void UpdateNearestArea()
        {
            if (PlayerEditing != null)
            {
                Visuals.NearestArea = NavigationMesh.GetAreaWithin(PlayerEditing.Camera.position)?.RoomKindArea;
            }
        }

        private void UpdateFacingArea()
        {
            if (PlayerEditing != null)
            {
                var room = RoomIdUtils.RoomAtPositionRaycasts(PlayerEditing.Position);

                var localPosition = room.transform.InverseTransformPoint(PlayerEditing.Camera.position);
                var localForward = room.transform.InverseTransformDirection(PlayerEditing.Camera.forward);

                Visuals.FacingArea = FindClosestAreaFacingAt((room.Name, room.Shape, (RoomZone)room.Zone), localPosition, localForward);
            }
        }

        #region Private constructor
        private NavigationMeshEditor()
        { }
        #endregion

        private IEnumerator<float> RunEachFrame(Action action)
        {
            while (true)
            {
                action.Invoke();

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}
