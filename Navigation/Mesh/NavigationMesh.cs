using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SCPSLBot.Navigation.Graph;
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

        public Dictionary<(RoomName, RoomShape, RoomZone), List<RoomKindVertex>> VerticesByRoomKind { get; } = new();
        public Dictionary<FacilityRoom, List<RoomVertex>> VerticesByRoom { get; } = new();

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

        public List<Area> GetShortestPath(Area startingArea, Area endArea)
        {
            var areasWithPriorityToEvaluate = new Dictionary<Area, float>();
            var cameFromAreas = new Dictionary<Area, Area>();
            var areaCosts = new Dictionary<Area, float>();

            var cost = 0f;
            var heuristic = Vector3.Magnitude(endArea.CenterPosition - startingArea.CenterPosition);

            areasWithPriorityToEvaluate.Add(startingArea, cost + heuristic);
            cameFromAreas.Add(startingArea, null);
            areaCosts.Add(startingArea, cost);

            var area = startingArea;

            do
            {
                area = areasWithPriorityToEvaluate.OrderBy(p => p.Value).First().Key;
                areasWithPriorityToEvaluate.Remove(area);

                if (area == endArea)
                {
                    break;
                }

                cost = areaCosts[area];

                foreach (var connectedArea in area.ConnectedAreas)
                {
                    var newCost = cost + Vector3.Magnitude(connectedArea.CenterPosition - area.CenterPosition);

                    if (!areaCosts.ContainsKey(connectedArea) || newCost < areaCosts[connectedArea])
                    {
                        areaCosts[connectedArea] = newCost;
                        heuristic = Vector3.Magnitude(endArea.CenterPosition - connectedArea.CenterPosition);
                        areasWithPriorityToEvaluate[connectedArea] = newCost + heuristic;
                        cameFromAreas[connectedArea] = area;
                    }
                }
            }
            while (areasWithPriorityToEvaluate.Any());

            var shortestPath = new List<Area>();

            if (cameFromAreas.ContainsKey(endArea))
            {
                area = endArea;
                while (area != null)
                {
                    shortestPath.Add(area);
                    cameFromAreas.TryGetValue(area, out area);
                }
                shortestPath.Reverse();
            }

            return shortestPath;
        }

        public RoomKindArea AddArea(IEnumerable<Vector3> vertexLocalPositions, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                roomKindAreas = new List<RoomKindArea>();
                AreasByRoomKind.Add(roomKind, roomKindAreas);
            }

            var newRoomKindArea = new RoomKindArea(vertexLocalPositions.Select(p => new RoomKindVertex(p)))
            {
                RoomKind = roomKind,
            };

            roomKindAreas.Add(newRoomKindArea);

            foreach (var roomOfKind in AreasByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                roomOfKind.Value.Add(new Area(newRoomKindArea, roomOfKind.Key));
            }

            return newRoomKindArea;
        }

        public void RemoveArea(RoomKindArea roomKindArea, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                Log.Warning($"No nodes at room {roomKind} to remove node from.");
                return;
            }

            foreach (var connectedToRemovingArea in roomKindArea.ConnectedRoomKindAreas)
            {
                connectedToRemovingArea.ConnectedRoomKindAreas.Remove(roomKindArea);
            }

            roomKindAreas.Remove(roomKindArea);

            foreach (var roomOfKind in AreasByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                var node = roomOfKind.Value.Find(n => n.RoomKindArea == roomKindArea);
                roomOfKind.Value.Remove(node);
            }
        }

        private bool IsPointWithinArea(Area area, Vector3 pointLocalPosition)
        {
            var areaRoomKindEdges = area.RoomKindArea.Edges;

            return areaRoomKindEdges.Select(e => (
                    dirTo2: e.To.LocalPosition - e.From.LocalPosition, 
                    dirToPoint: pointLocalPosition - e.From.LocalPosition))
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
