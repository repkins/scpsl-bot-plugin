﻿using MapGeneration;
using MEC;
using PluginAPI.Core;
using SCPSLBot.Commands.Navigation;
using SCPSLBot.MapGeneration;
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
        private bool AutoSelectModeEnabled = false;

        public void Init()
        {
            Visuals.SelectedVertices = SeletedVertices;
            Visuals.Init();

            Timing.RunCoroutine(RunEachFrame(UpdateEditing));

            Timing.RunCoroutine(RunEachFrame(UpdateNearestVertex));
            Timing.RunCoroutine(RunEachFrame(UpdateFacingVertex));
            Timing.RunCoroutine(RunEachFrame(UpdateVertexAutoSelect));

            Timing.RunCoroutine(RunEachFrame(UpdateNearestArea));
            Timing.RunCoroutine(RunEachFrame(UpdateCachedArea));
            Timing.RunCoroutine(RunEachFrame(UpdateFacingArea));

            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateBroadcastMessage));

            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateVertexVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateAreaVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateEdgeVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateConnectionVisuals));

        }

        public RoomKindVertex FindClosestVertexFacingAt((RoomName, RoomShape, RoomZone) roomKind, Vector3 localPosition, Vector3 localDirection)
        {
            if (!NavigationMesh.Instance.VerticesByRoomKind.TryGetValue(roomKind, out var roomKindVertices))
            {
                return null;
            }

            var targetVertex = roomKindVertices
                .Select(a => (n: a, d: Vector3.SqrMagnitude(a.LocalPosition - localPosition)))
                .Where(t => t.d < 50f && t.d > 1f)
                .OrderBy(t => t.d)
                .Select(t => t.n)
                .FirstOrDefault(a => Vector3.Dot(Vector3.Normalize(a.LocalPosition - localPosition), localDirection) > 0.999848f);

            return targetVertex;
        }

        public RoomKindArea FindClosestAreaByCenter(Vector3 position, float radius = 1f)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!room || !NavigationMesh.AreasByRoomKind.TryGetValue((room.Name, room.Shape, (RoomZone)room.Zone), out var roomAreas))
            {
                return null;
            }

            var radiusSqr = Mathf.Pow(radius, 2);
            var localPosition = room.transform.InverseTransformPoint(position);

            var areasWithinRadius = roomAreas.Select(area => (area, distSqr: Vector3.SqrMagnitude(area.LocalCenterPosition - localPosition)))
                .Where(t => t.distSqr < radiusSqr);

            if (!areasWithinRadius.Any())
            {
                return null;
            }

            return areasWithinRadius
                .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                .area;
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

            if (SeletedVertices.Count == 2)
            {
                var lineSegment = (from: SeletedVertices.First(), to: SeletedVertices.Last());
                
                var dirTo2 = (lineSegment.to.LocalPosition - lineSegment.from.LocalPosition);
                var dirToPoint = (localPosition - lineSegment.from.LocalPosition);
                var dirToProj = (Vector3.Project(dirToPoint, dirTo2));
                var projected = (dirToProj + lineSegment.from.LocalPosition);

                localPosition = projected;
            }

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

            SeletedVertices.Remove(vertex.RoomKindVertex);

            if (!NavigationMesh.DeleteVertex(vertex.RoomKindVertex))
            {
                return false;
            }

            Log.Info($"Vertex at local position {vertex.RoomKindVertex.LocalPosition} removed under room {roomKind}.");

            return true;
        }

        public bool MoveVertex(Vector3 position)
        {
            var vertex = NavigationMesh.GetNearbyVertex(position)?.RoomKindVertex;
            if (vertex == null)
            {
                Log.Info($"No vertex found nearby to move. Checking for selection.");

                if (!SeletedVertices.Any())
                {
                    Log.Warning($"No vertices selected to move.");
                    return false;
                }

                vertex = SeletedVertices.First();
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var localPosition = room.transform.InverseTransformPoint(position);

            if (!NavigationMesh.MoveVertex(vertex, localPosition))
            {
                return false;
            }

            Log.Info($"Vertex #{NavigationMesh.VerticesByRoomKind[roomKind].IndexOf(vertex)} of room kind {roomKind} moved to new local position {vertex.LocalPosition}.");

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

        public void ToggleAutoSelectingVertices(bool isEnabled)
        {
            AutoSelectModeEnabled = isEnabled;
        }

        public RoomKindArea MakeArea(Vector3 position)
        {
            if (SeletedVertices.Count < 2)
            {
                Log.Warning($"Not enough vertices (min 2) selected.");
                return null;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var newArea = NavigationMesh.MakeArea(SeletedVertices, roomKind);

            Log.Info($"Area #{NavigationMesh.AreasByRoomKind[roomKind].IndexOf(newArea)} at local center position {newArea.LocalCenterPosition} added under room {roomKind}.");

            SeletedVertices.Clear();
            AutoSelectModeEnabled = false;
            PlayerEditing.ReceiveHint($"<size=30>Vertex auto-selection is stopped on area creation.", 3f);

            return newArea;
        }

        public bool DissolveArea(Vector3 position)
        {
            var area = Visuals.NearestArea;
            if (area == null)
            {
                Log.Warning($"No area found within to remove.");

                return false;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            NavigationMesh.RemoveArea(area);

            Log.Info($"Area at local center position {area.LocalCenterPosition} removed under room {roomKind}.");

            return true;
        }

        public bool CreateVertexOnClosestEdge(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var localPosition = room.transform.InverseTransformPoint(position);

            if (!NavigationMesh.AreasByRoomKind.ContainsKey(roomKind))
            {
                return false;
            }

            var (newVertexPos, area, edge) = NavigationMesh.AreasByRoomKind[roomKind]
                .SelectMany(a => a.Edges.Select(e => (edge: (from: e.From, to: e.To), area: a)))
                .Select(t => (
                    t.edge,
                    dirTo2: (t.edge.to.LocalPosition - t.edge.from.LocalPosition),
                    dirToPoint: (localPosition - t.edge.from.LocalPosition),
                    t.area))
                .Select(t => (t.edge, t.dirTo2, dirToProj: (Vector3.Project(t.dirToPoint, t.dirTo2)), t.area))
                .Where(t => Vector3.Dot(t.dirToProj, t.dirTo2) > 0f && t.dirToProj.sqrMagnitude < t.dirTo2.sqrMagnitude)
                .Select(t => (projected: (t.dirToProj + t.edge.from.LocalPosition), t.area, t.edge))

                .OrderBy(t => Vector3.SqrMagnitude(t.projected - localPosition))
                .FirstOrDefault();

            if (area == null)
            {
                return false;
            }

            var vertex = NavigationMesh.AddVertex(newVertexPos, roomKind);

            NavigationMesh.AddVertexToArea(area, vertex, edge.to);

            Log.Info($"Vertex #{NavigationMesh.VerticesByRoomKind[roomKind].IndexOf(vertex)} created on edge of area #{NavigationMesh.AreasByRoomKind[roomKind].IndexOf(area)}");

            return true;
        }

        public bool SliceClosestAreaEdge(Vector3 position, Vector3 direction)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);
            var roomKind = (room.Name, room.Shape, (RoomZone)room.Zone);

            var localPosition = room.transform.InverseTransformPoint(position);
            var localDirection = room.transform.InverseTransformDirection(direction);

            if (!NavigationMesh.AreasByRoomKind.ContainsKey(roomKind))
            {
                return false;
            }

            var lookPlane = new Plane(Vector3.Cross(localDirection, Vector3.up), localPosition);

            var (newVertexPos, area, edge) = NavigationMesh.AreasByRoomKind[roomKind]
                .SelectMany(a => a.Edges.Select(e => (edge: (from: e.From, to: e.To), area: a)))
                .Select(t => (
                    t.edge,
                    dirTo2: (t.edge.to.LocalPosition - t.edge.from.LocalPosition),
                    t.area))
                .Select(t => (
                    t.edge, 
                    t.dirTo2, 
                    rayTo2: new Ray(t.edge.from.LocalPosition, t.dirTo2), 
                    t.area))
                .Select(t => (
                    t.edge, 
                    t.dirTo2, 
                    t.rayTo2,
                    isHit: lookPlane.Raycast(t.rayTo2, out var distToHit),
                    distToHit,
                    t.area))
                .Where(t => t.isHit)
                .Select(t => (
                    t.edge, 
                    t.dirTo2,
                    hitPoint: t.rayTo2.GetPoint(t.distToHit),
                    t.area))
                .Select(t => (
                    t.edge, 
                    t.dirTo2,
                    t.hitPoint,
                    dirToHit: t.hitPoint - t.edge.from.LocalPosition,
                    t.area))
                .Where(t => Vector3.Dot(t.dirToHit, t.dirTo2) > 0f && t.dirToHit.sqrMagnitude < t.dirTo2.sqrMagnitude)

                .OrderBy(t => Vector3.SqrMagnitude(t.hitPoint - localPosition))
                .Select(t => (t.hitPoint, t.area, t.edge))
                .FirstOrDefault();

            if (area == null)
            {
                return false;
            }

            var vertex = NavigationMesh.AddVertex(newVertexPos, roomKind);

            NavigationMesh.AddVertexToArea(area, vertex, edge.to);

            Log.Info($"Vertex #{NavigationMesh.VerticesByRoomKind[roomKind].IndexOf(vertex)} created on edge of area #{NavigationMesh.AreasByRoomKind[roomKind].IndexOf(area)}");

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

            var path = new List<Area>();
            NavigationMesh.FindShortestPath(CachedArea, targetArea, path);
            if (path.Count == 0)
            {
                Log.Warning($"No path found.");
            }

            Visuals.Path.Clear();
            Visuals.Path.AddRange(path);

            return true;
        }

        public bool CreateConnection(Vector3 position)
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

            NavigationMesh.CreateConnection(CachedArea.RoomKindArea, targetArea.RoomKindArea);

            return true;
        }

        public bool DeleteConnection(Vector3 position)
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

            NavigationMesh.DeleteConnection(CachedArea.RoomKindArea, targetArea.RoomKindArea);

            return true;
        }

        private void UpdateEditing()
        {
            if (PlayerEditing != LastPlayerEditing)
            {
                LastPlayerEditing = PlayerEditing;

                Visuals.PlayerEnabledVisualsFor = PlayerEditing;

                Log.Debug($"Visuals.PlayerEnabledVisualsFor.DisplayNickname = {Visuals.PlayerEnabledVisualsFor?.DisplayNickname}");
            }
        }

        private void UpdateNearestVertex()
        {
            if (PlayerEditing != null)
            {
                Visuals.NearestVertex = NavigationMesh.GetNearbyVertex(PlayerEditing.Position, .125f)?.RoomKindVertex;
            }
        }

        private void UpdateFacingVertex()
        {
            if (PlayerEditing != null)
            {
                var room = RoomIdUtils.RoomAtPositionRaycasts(PlayerEditing.Position);

                var localPosition = room.transform.InverseTransformPoint(PlayerEditing.Camera.position);
                var localForward = room.transform.InverseTransformDirection(PlayerEditing.Camera.forward);

                Visuals.FacingVertex = FindClosestVertexFacingAt((room.Name, room.Shape, (RoomZone)room.Zone), localPosition, localForward);
            }
        }

        private void UpdateNearestArea()
        {
            if (PlayerEditing != null)
            {
                var playerPosition = PlayerEditing.Position;
                Visuals.NearestArea = NavigationMesh.GetAreaWithin(playerPosition)?.RoomKindArea ?? FindClosestAreaByCenter(playerPosition, .25f);
            }
        }

        private void UpdateCachedArea()
        {
            if (PlayerEditing != null)
            {
                Visuals.CachedArea = CachedArea?.RoomKindArea;
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

        private void UpdateVertexAutoSelect()
        {
            if (PlayerEditing != null && AutoSelectModeEnabled && Visuals.NearestVertex != null)
            {
                if (!SeletedVertices.Contains(Visuals.NearestVertex))
                {
                    SeletedVertices.Add(Visuals.NearestVertex);
                }
                else if (SeletedVertices.Count > 1 && SeletedVertices.FirstOrDefault() == Visuals.NearestVertex)
                {
                    AutoSelectModeEnabled = false;
                    PlayerEditing.ReceiveHint($"<size=30>Vertex auto-selection is stopped on first vertex selected.", 3f);

                    Log.Info($"Vertex auto-selection stopped on first vertex selected.");
                }
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

        private IEnumerator<float> RunOncePerSecond(Action action)
        {
            while (true)
            {
                action.Invoke();

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
