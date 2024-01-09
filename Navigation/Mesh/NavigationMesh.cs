using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using SCPSLBot.MapGeneration;
using SCPSLBot.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RectTransform;

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

        public (RoomVertex From, RoomVertex To)? GetNearestEdge(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!room || !AreasByRoom.TryGetValue(room.ApiRoom, out var roomAreas))
            {
                return null;
            }

            var localPosition = room.transform.InverseTransformPoint(position);

            var roomKindEdge = roomAreas.SelectMany(a => a.RoomKindArea.Edges)
                .Select(e => (edge: e, dist: GetPointDistToEdge(e, localPosition)))
                .Where(t => t.dist > 0f)
                .Where(t => IsAlongEdge(t.edge, localPosition))
                .Where(t => IsEdgeCenterWithinVertically(t.edge, localPosition))
                .OrderBy(t => t.dist)
                .Select(t => new (RoomKindVertex From, RoomKindVertex To)?(t.edge))
                .DefaultIfEmpty(null)
                .First();

            if (roomKindEdge == null)
            {
                return null;
            }

            RoomVertex roomEdgeFrom = VerticesByRoom[room.ApiRoom].Find(v => v.RoomKindVertex == roomKindEdge.Value.From),
                       roomEdgeTo = VerticesByRoom[room.ApiRoom].Find(v => v.RoomKindVertex == roomKindEdge.Value.To);

            return (roomEdgeFrom, roomEdgeTo);
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

        public RoomVertex GetNearbyVertex(Vector3 position, float radius = 1f)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            if (!room || !VerticesByRoom.TryGetValue(room.ApiRoom, out var roomVertexs))
            {
                return null;
            }

            var radiusSqr = Mathf.Pow(radius, 2);
            var localPosition = room.transform.InverseTransformPoint(position);

            var verticesWithinRadius = roomVertexs.Select(vertex => (vertex, distSqr: Vector3.SqrMagnitude(vertex.RoomKindVertex.LocalPosition - localPosition)))
                .Where(t => t.distSqr < radiusSqr);

            if (!verticesWithinRadius.Any())
            {
                return null;
            }
            
            return verticesWithinRadius
                .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                .vertex;
        }

        public RoomKindVertex AddVertex(Vector3 localPosition, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!VerticesByRoomKind.TryGetValue(roomKind, out var roomKindVertices))
            {
                roomKindVertices = new List<RoomKindVertex>();
                VerticesByRoomKind.Add(roomKind, roomKindVertices);
            }

            var newRoomKindVertex = new RoomKindVertex(localPosition, roomKind);
            roomKindVertices.Add(newRoomKindVertex);

            foreach (var roomVerticesPair in VerticesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                Log.Debug($"Room vertex added.");
                roomVerticesPair.Value.Add(new RoomVertex(newRoomKindVertex, roomVerticesPair.Key));
            }

            return newRoomKindVertex;
        }

        public bool DeleteVertex(RoomKindVertex roomKindVertex)
        {
            var roomKind = roomKindVertex.RoomKind;

            if (!VerticesByRoomKind.TryGetValue(roomKind, out var roomKindVertices))
            {
                Log.Warning($"No vertices at room {roomKind} to remove vertex from.");
                return false;
            }

            if (AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                foreach (var area in roomKindAreas.ToArray())
                {
                    area.Vertices.Remove(roomKindVertex);
                    if (area.Vertices.Count < 3)
                    {
                        RemoveArea(area);

                        Log.Warning($"Area at local center position {area.LocalCenterPosition} removed under room {roomKind}.");
                    }
                }
            }

            roomKindVertices.Remove(roomKindVertex);

            foreach (var roomVerticesPair in VerticesByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                Log.Debug($"Room vertex removed.");
                var vertex = roomVerticesPair.Value.Find(n => n.RoomKindVertex == roomKindVertex);
                roomVerticesPair.Value.Remove(vertex);
            }

            return true;
        }

        public bool MoveVertex(RoomKindVertex roomKindVertex, Vector3 newLocalPosition)
        {
            roomKindVertex.LocalPosition = newLocalPosition;

            return true;
        }

        public RoomKindArea MakeArea(IEnumerable<RoomKindVertex> roomKindVertices, (RoomName, RoomShape, RoomZone) roomKind)
        {
            if (!AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                roomKindAreas = new List<RoomKindArea>();
                AreasByRoomKind.Add(roomKind, roomKindAreas);
            }

            var newRoomKindArea = new RoomKindArea(roomKindVertices, roomKind);
            roomKindAreas.Add(newRoomKindArea);

            foreach (var edge in newRoomKindArea.Edges)
            {
                var inversedEdge = (edge.To, edge.From);
                var connectedArea = roomKindAreas.Find(a => a.Edges.Contains(inversedEdge));
                if (connectedArea != null)
                {
                    newRoomKindArea.ConnectedRoomKindAreas.Add(connectedArea);
                    connectedArea.ConnectedRoomKindAreas.Add(newRoomKindArea);
                }
            }

            AddRoomAreas(newRoomKindArea);

            return newRoomKindArea;
        }

        public void RemoveArea(RoomKindArea roomKindArea)
        {
            var roomKind = roomKindArea.RoomKind;

            if (!AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
            {
                Log.Warning($"No areas at room {roomKind} to remove area from.");
                return;
            }

            foreach (var connectedToRemovingArea in roomKindArea.ConnectedRoomKindAreas)
            {
                connectedToRemovingArea.ConnectedRoomKindAreas.Remove(roomKindArea);
            }

            roomKindAreas.Remove(roomKindArea);

            foreach (var roomOfKind in AreasByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                var area = roomOfKind.Value.Find(n => n.RoomKindArea == roomKindArea);
                roomOfKind.Value.Remove(area);

                foreach (var connectedArea in area.ConnectedAreas)
                {
                    var connectedConnectedArea = connectedArea.ConnectedAreas as List<Area>;
                    //connectedConnectedArea.Remove(area);
                }
            }
        }

        public void ReadMesh(BinaryReader binaryReader)
        {
            var version = binaryReader.ReadByte();

            var roomCount = binaryReader.ReadInt32();

            for (var i = 0; i < roomCount; i++)
            {
                Enum.TryParse<RoomName>(binaryReader.ReadString(), out var roomName);
                Enum.TryParse<RoomShape>(binaryReader.ReadString(), out var roomShape);
                Enum.TryParse<RoomZone>(binaryReader.ReadString(), out var roomZone);
                var roomKind = (roomName, roomShape, roomZone);

                ///
                /// Vertices reading
                /// 

                if (!VerticesByRoomKind.TryGetValue(roomKind, out var roomKindVertices))
                {
                    roomKindVertices = new List<RoomKindVertex>();
                    VerticesByRoomKind.Add(roomKind, roomKindVertices);
                }
                else
                {
                    roomKindVertices.Clear();
                }

                var vertexCount = binaryReader.ReadInt32();

                for (var j = 0; j < vertexCount; j++)
                {
                    var vertexLocalPosition = new Vector3()
                    {
                        x = binaryReader.ReadSingle(),
                        y = binaryReader.ReadSingle(),
                        z = binaryReader.ReadSingle()
                    };

                    var newRoomKindVertex = new RoomKindVertex(vertexLocalPosition, roomKind);
                    roomKindVertices.Add(newRoomKindVertex);
                }

                ///
                /// Areas reading
                /// 

                if (!AreasByRoomKind.TryGetValue(roomKind, out var roomKindAreas))
                {
                    roomKindAreas = new List<RoomKindArea>();
                    AreasByRoomKind.Add(roomKind, roomKindAreas);
                }
                else
                {
                    roomKindAreas.Clear();
                }

                var areasCount = binaryReader.ReadInt32();

                var areasVertices = new int[areasCount][];
                var areasConnections = new int[areasCount][];

                for (var j = 0; j < areasCount; j++)
                {
                    var newRoomKindArea = new RoomKindArea()
                    {
                        RoomKind = roomKind,
                    };
                    roomKindAreas.Add(newRoomKindArea);

                    var areaVerticesCount = binaryReader.ReadInt32();
                    var areaVertices = new int[areaVerticesCount];
                    for (var k = 0; k < areaVerticesCount; k++)
                    {
                        areaVertices[k] = binaryReader.ReadInt32();
                    }
                    areasVertices[j] = areaVertices;

                    var connectedAreasCount = binaryReader.ReadInt32();
                    var connectedAreas = new int[connectedAreasCount];
                    for (var k = 0; k < connectedAreasCount; k++)
                    {
                        connectedAreas[k] = binaryReader.ReadInt32();
                    }                    
                    areasConnections[j] = connectedAreas;
                }

                foreach (var (area, vertices) in areasVertices.Select((vertices, areaIndex) => (roomKindAreas[areaIndex], vertices)))
                {
                    area.Vertices.AddRange(vertices.Select(vertexIdx => roomKindVertices[vertexIdx]));
                }

                foreach (var (area, conns) in areasConnections.Select((conns, areaIndex) => (roomKindAreas[areaIndex], conns)))
                {
                    area.ConnectedRoomKindAreas.AddRange(conns.Select(connectedIndex => roomKindAreas[connectedIndex]));
                }
            }
        }

        public void WriteMesh(BinaryWriter binaryWriter)
        {
            byte version = 1;
            binaryWriter.Write(version);
            
            binaryWriter.Write(VerticesByRoomKind.Count);

            foreach (var (roomKind, vertices) in VerticesByRoomKind.Select(p => (roomKind: p.Key, vertices: p.Value)))
            {
                var (roomName, roomShape, roomZone) = roomKind;

                binaryWriter.Write(roomName.ToString());
                binaryWriter.Write(roomShape.ToString());
                binaryWriter.Write(roomZone.ToString());

                binaryWriter.Write(vertices.Count);
                foreach (var vertex in vertices)
                {
                    binaryWriter.Write(vertex.LocalPosition.x);
                    binaryWriter.Write(vertex.LocalPosition.y);
                    binaryWriter.Write(vertex.LocalPosition.z);
                }

                if (!AreasByRoomKind.TryGetValue(roomKind, out var areas))
                {
                    areas = new();
                }

                binaryWriter.Write(areas.Count);
                foreach (var area in areas)
                {
                    binaryWriter.Write(area.Vertices.Count);
                    foreach (var vertexIdx in area.Vertices.Select(areaVertex => VerticesByRoomKind[roomKind].IndexOf(areaVertex)))
                    {
                        binaryWriter.Write(vertexIdx);
                    }

                    binaryWriter.Write(area.ConnectedRoomKindAreas.Count);
                    foreach (var connIdx in area.ConnectedRoomKindAreas.Select(connArea => AreasByRoomKind[roomKind].IndexOf(connArea)))
                    {
                        binaryWriter.Write(connIdx);
                    }
                }
            }
        }

        public void InitRoomVertices()
        {
            foreach (var room in Facility.Rooms)
            {
                var vertices = new List<RoomVertex>();
                VerticesByRoom.Add(room, vertices);

                if (!VerticesByRoomKind.TryGetValue((room.Identifier.Name, room.Identifier.Shape, (RoomZone)room.Identifier.Zone), out var roomKindVertices))
                {
                    continue;
                }

                vertices.AddRange(roomKindVertices.Select(k => new RoomVertex(k, room)));
            }
        }

        public void ResetVertices()
        {
            VerticesByRoom.Clear();
        }

        public void InitRoomAreas()
        {
            foreach (var room in Facility.Rooms)
            {
                var areas = new List<Area>();
                AreasByRoom.Add(room, areas);

                if (!AreasByRoomKind.TryGetValue((room.Identifier.Name, room.Identifier.Shape, (RoomZone)room.Identifier.Zone), out var roomKindAreas))
                {
                    continue;
                }

                areas.AddRange(roomKindAreas.Select(k => new Area(k, room)));

                foreach (var roomArea in areas)
                {
                    var connectedAreas = roomArea.RoomKindArea.ConnectedRoomKindAreas.Select(c => AreasByRoom[room].Find(a => a.RoomKindArea == c));
                    //roomArea.ConnectedAreas.AddRange(connectedAreas);
                }
            }
        }

        public void ResetAreas()
        {
            AreasByRoom.Clear();
        }

        public void AddVertexToArea(RoomKindArea area, RoomKindVertex vertex, RoomKindVertex beforeVertex)
        {
            var atIdx = area.Vertices.IndexOf(beforeVertex);

            area.Vertices.Insert(atIdx, vertex);
        }

        private bool IsPointWithinArea(Area area, Vector3 pointLocalPosition)
        {
            var areaRoomKindEdges = area.RoomKindArea.Edges;

            var isAnyVertexWithinVerticalRange = false;
            foreach (var e in areaRoomKindEdges)
            {
                if (GetPointDistToEdge(e, pointLocalPosition) <= 0f)
                {
                    return false;
                }

                if (!isAnyVertexWithinVerticalRange)
                {
                    isAnyVertexWithinVerticalRange = 
                        e.From.LocalPosition.y > pointLocalPosition.y - 1f
                        && e.From.LocalPosition.y < pointLocalPosition.y + 1f;
                }
            }

            return isAnyVertexWithinVerticalRange;
        }

        private float GetPointDistToEdge((RoomKindVertex From, RoomKindVertex To) edge, Vector3 localPoint)
        {
            var dirTo2 = edge.To.LocalPosition - edge.From.LocalPosition;
            var dirToPoint = localPoint - edge.From.LocalPosition;

            var edgeNormal = Vector3.Cross(dirTo2.normalized, Vector3.down);

            var p = Vector3.Dot(edgeNormal, dirToPoint);
            return p;
        }

        private bool IsAlongEdge((RoomKindVertex From, RoomKindVertex To) edge, Vector3 localPoint)
        {
            var dir1To2 = edge.To.LocalPosition - edge.From.LocalPosition;
            var dir1ToPoint = localPoint - edge.From.LocalPosition;

            var dir2To1 = edge.From.LocalPosition - edge.To.LocalPosition;
            var dir2ToPoint = localPoint - edge.To.LocalPosition;

            return Vector3.Dot(dir1ToPoint, dir1To2) > 0f && Vector3.Dot(dir2ToPoint, dir2To1) > 0f;
        }

        private bool IsEdgeCenterWithinVertically((RoomKindVertex From, RoomKindVertex To) edge, Vector3 localPoint)
        {
            var localPointYLowest = localPoint.y - 1f;
            var localPointYHighest = localPoint.y + 1f;
            var edgeCenter = Vector3.Lerp(edge.From.LocalPosition, edge.To.LocalPosition, 0.5f);

            return edgeCenter.y > localPointYLowest
                && edgeCenter.y < localPointYHighest;
        }

        private void AddRoomAreas(RoomKindArea roomKindArea)
        {
            var roomsAreasOfRoomKind = AreasByRoom.Select(r => (room: r.Key, areas: r.Value))
                .Where(t => (t.room.Identifier.Name, t.room.Identifier.Shape, (RoomZone)t.room.Identifier.Zone) == roomKindArea.RoomKind);

            foreach (var (room, areas) in roomsAreasOfRoomKind)
            {
                var newRoomArea = new Area(roomKindArea, room);
                areas.Add(newRoomArea);

                var connectedAreas = roomKindArea.ConnectedRoomKindAreas.Select(c => AreasByRoom[room].Find(a => a.RoomKindArea == c));
                //newRoomArea.ConnectedAreas.AddRange(connectedAreas);
            }
        }

        #region Private constructor
        private NavigationMesh()
        { }
        #endregion
    }
}
