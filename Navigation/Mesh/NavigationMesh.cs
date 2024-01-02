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
                foreach (var areaVertices in roomKindAreas.Select(a => a.Vertices))
                {
                    if (areaVertices.Count == 3)
                    {
                        Log.Warning($"Can't remove vertex from triangle area.");
                        return false;
                    }
                    areaVertices.Remove(roomKindVertex);
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

            foreach (var roomAreasPair in AreasByRoom.Where(r => (r.Key.Identifier.Name, r.Key.Identifier.Shape, (RoomZone)r.Key.Identifier.Zone) == roomKind))
            {
                var newRoomArea = new Area(newRoomKindArea, roomAreasPair.Key);
                roomAreasPair.Value.Add(newRoomArea);

                var connectedAreas = newRoomKindArea.ConnectedRoomKindAreas.Select(c => AreasByRoom[roomAreasPair.Key].Find(a => a.RoomKindArea == c));
                //newRoomArea.ConnectedAreas.AddRange(connectedAreas);
            }

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
