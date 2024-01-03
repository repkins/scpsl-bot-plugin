using AdminToys;
using Mirror;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RectTransform;

namespace SCPSLBot.Navigation.Mesh
{
    internal class NavigationMeshVisuals
    {
        public Player PlayerEnabledVisualsFor { get; set; }

        public RoomKindVertex NearestVertex { get; set; }
        public RoomKindVertex FacingVertex { get; set; }

        public List<RoomKindVertex> SelectedVertices { get; set; }

        public RoomKindArea NearestArea { get; set; }
        public RoomKindArea FacingArea { get; set; }

        public List<Area> Path { get; } = new List<Area>();

        private Dictionary<RoomVertex, PrimitiveObjectToy> VertexVisuals { get; } = new();
        private Dictionary<(RoomKindVertex From, RoomKindVertex To), (PrimitiveObjectToy, Area)> EdgeVisuals { get; } = new();

        private Dictionary<Area, PrimitiveObjectToy> AreaVisuals { get; } = new ();

        private NavigationMesh NavigationMesh { get; } = NavigationMesh.Instance;

        private RoomKindVertex LastNearestVertex { get; set; }
        private RoomKindVertex LastFacingVertex { get; set; }

        private RoomKindArea LastNearestArea { get; set; }
        private RoomKindArea LastFacingArea { get; set; }

        private string[] VisualsMessages { get; } = new string[2];

        private string SentBroadcastMessage;

        private float verticalOffset = 0f;

        public void UpdateVertexInfo()
        {
            if (PlayerEnabledVisualsFor != null)
            {
                if (NearestVertex != LastNearestVertex)
                {
                    LastNearestVertex = NearestVertex;

                    if (NearestVertex != null)
                    {
                        var nearestVertexId = NavigationMesh.VerticesByRoomKind[NearestVertex.RoomKind].IndexOf(NearestVertex);
                        VisualsMessages[0] = $"Vertex #{nearestVertexId} in {NearestVertex.RoomKind}";

                        var selectedIdx = SelectedVertices.IndexOf(NearestVertex);
                        if (selectedIdx >= 0)
                        {
                            VisualsMessages[0] += $" (Selected #{selectedIdx})";
                        }
                    }
                    else
                    {
                        VisualsMessages[0] = null;
                    }
                }

                if (FacingVertex != LastFacingVertex)
                {
                    LastFacingVertex = FacingVertex;

                    if (FacingVertex != null)
                    {
                        var facingVertexId = NavigationMesh.VerticesByRoomKind[FacingVertex.RoomKind].IndexOf(FacingVertex);
                        VisualsMessages[1] = $"Facing vertex #{facingVertexId} in {FacingVertex.RoomKind}";

                        var selectedIdx = SelectedVertices.IndexOf(FacingVertex);
                        if (selectedIdx >= 0)
                        {
                            VisualsMessages[1] += $" (selected #{selectedIdx})";
                        }
                    }
                    else
                    {
                        VisualsMessages[1] = null;
                    }
                }
            }
        }

        public void UpdateAreaInfo()
        {
            if (PlayerEnabledVisualsFor != null)
            {
                var nearestArea = NearestArea;

                if (nearestArea != LastNearestArea)
                {
                    LastNearestArea = nearestArea;

                    if (nearestArea != null)
                    {
                        //var connectedIdsStr = string.Join(", ", nearestArea.ConnectedAreas.Select(c => $"#{c.Id}"));
                        var nearestAreaId = NavigationMesh.AreasByRoomKind[nearestArea.RoomKind].IndexOf(nearestArea);
                        VisualsMessages[0] = $"Area #{nearestAreaId} in {nearestArea.RoomKind}";
                    }
                    else
                    {
                        VisualsMessages[0] = null;
                    }
                }

                if (FacingArea != LastFacingArea)
                {
                    LastFacingArea = FacingArea;

                    if (FacingArea != null)
                    {
                        var facingAreaId = NavigationMesh.AreasByRoomKind[FacingArea.RoomKind].IndexOf(FacingArea);
                        VisualsMessages[1] = $"Facing area #{facingAreaId} in {FacingArea.RoomKind}";

                        if (NearestArea != null)
                        {
                            if (NearestArea.ConnectedRoomKindAreas.Contains(FacingArea) && FacingArea.ConnectedRoomKindAreas.Contains(NearestArea))
                            {
                                VisualsMessages[1] += $" <color=green>(bi-connected)";
                            }
                            else if (NearestArea.ConnectedRoomKindAreas.Contains(FacingArea))
                            {
                                VisualsMessages[1] += $" <color=green>(connected to)";
                            }
                            else if (FacingArea.ConnectedRoomKindAreas.Contains(NearestArea))
                            {
                                VisualsMessages[1] += $" <color=green>(connected from)";
                            }
                        }
                    }
                    else
                    {
                        VisualsMessages[1] = null;
                    }
                }
            }
        }

        public void UpdateBroadcastMessage()
        {
            var messageLinesToSend = VisualsMessages.Where(m => m != null);
            if (messageLinesToSend.Any())
            {
                var broadcastMessage = string.Join("\n", messageLinesToSend);
                PlayerEnabledVisualsFor.SendBroadcast($"<size=30>{broadcastMessage}", 60, shouldClearPrevious: true);
                SentBroadcastMessage = broadcastMessage;
            }
            else
            {
                if (SentBroadcastMessage != null)
                {
                    PlayerEnabledVisualsFor.ClearBroadcasts();
                    SentBroadcastMessage = null;
                }
            }
        }

        public void UpdateAreaVisuals()
        {
            if (PlayerEnabledVisualsFor != null)
            {
                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var areaVisual in AreaVisuals.ToArray())
                {
                    if (!NavigationMesh.AreasByRoom.Values.Any(l => l.Contains(areaVisual.Key)))
                    {
                        NetworkServer.Destroy(areaVisual.Value.gameObject);
                        AreaVisuals.Remove(areaVisual.Key);
                    }
                }

                foreach (var area in NavigationMesh.AreasByRoom.Values.SelectMany(l => l))
                {
                    var room = area.Room.Identifier;

                    if (!AreaVisuals.TryGetValue(area, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        visual.NetworkPrimitiveType = PrimitiveType.Quad;

                        visual.transform.position = room.transform.TransformPoint(area.LocalCenterPosition);
                        visual.transform.RotateAround(visual.transform.position, visual.transform.right, -90f);
                        visual.transform.localScale = -Vector3.one * .25f;

                        visual.transform.position += Vector3.up * verticalOffset;

                        NetworkServer.Spawn(visual.gameObject);

                        AreaVisuals.Add(area, visual);
                    }

                    visual.NetworkMaterialColor = (NearestArea == area.RoomKindArea) ? Color.yellow : Color.white;

                }

                foreach (var areaInPath in Path)
                {
                    var areaVisual = AreaVisuals[areaInPath];
                    areaVisual.NetworkMaterialColor = Color.blue;
                }
            }
            else
            {
                foreach (var areaVisual in AreaVisuals.Values)
                {
                    NetworkServer.Destroy(areaVisual.gameObject);
                }
                AreaVisuals.Clear();
            }
        }

        public void UpdateVertexVisuals()
        {
            if (PlayerEnabledVisualsFor != null)
            {
                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var vertexVisual in VertexVisuals.ToArray())
                {
                    if (!NavigationMesh.VerticesByRoom.Values.Any(l => l.Contains(vertexVisual.Key)))
                    {
                        NetworkServer.Destroy(vertexVisual.Value.gameObject);
                        VertexVisuals.Remove(vertexVisual.Key);
                    }
                }

                foreach (var vertex in NavigationMesh.VerticesByRoom.Values.SelectMany(l => l))
                {
                    var room = vertex.Room.Identifier;

                    if (!VertexVisuals.TryGetValue(vertex, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        NetworkServer.Spawn(visual.gameObject);

                        visual.transform.position = vertex.Position;
                        visual.transform.localScale = -Vector3.one * 0.125f;

                        visual.transform.position += Vector3.up * verticalOffset;

                        VertexVisuals.Add(vertex, visual);
                    }

                    visual.NetworkMaterialColor = (NearestArea?.Vertices.Contains(vertex.RoomKindVertex) ?? false) ? Color.yellow : Color.white;
                }
            }
            else
            {
                foreach (var vertexVisual in VertexVisuals.Values)
                {
                    NetworkServer.Destroy(vertexVisual.gameObject);
                }
                VertexVisuals.Clear();
            }
        }

        public void UpdateEdgeVisuals()
        {
            if (PlayerEnabledVisualsFor != null)
            {
                foreach (var (edge, (visual, area)) in EdgeVisuals.Select(p => (p.Key, p.Value)).ToArray())
                {
                    var isAreaRemoved = !NavigationMesh.AreasByRoom[area.Room].Contains(area);

                    if (isAreaRemoved || (!area.RoomKindArea.Edges.Contains(edge)))
                    {
                        NetworkServer.Destroy(visual.gameObject);
                        EdgeVisuals.Remove(edge);
                    }
                }

                var primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);

                foreach (var area in NavigationMesh.AreasByRoom.Values.SelectMany(l => l))
                {
                    var room = area.Room.Identifier;

                    foreach (var edge in area.RoomKindArea.Edges)
                    {
                        if (!EdgeVisuals.TryGetValue(edge, out var edgeVisualArea))
                        {
                            var (newEdgeVisual,_) = edgeVisualArea;

                            newEdgeVisual = UnityEngine.Object.Instantiate(primPrefab);
                            newEdgeVisual.NetworkPrimitiveType = PrimitiveType.Cylinder;
                            newEdgeVisual.transform.position = Vector3.Lerp(room.transform.TransformPoint(edge.From.LocalPosition), room.transform.TransformPoint(edge.To.LocalPosition), 0.5f);
                            newEdgeVisual.transform.LookAt(room.transform.TransformPoint(edge.To.LocalPosition));
                            newEdgeVisual.transform.RotateAround(newEdgeVisual.transform.position, newEdgeVisual.transform.right, 90f);
                            newEdgeVisual.transform.localScale = -Vector3.forward * 0.01f + -Vector3.right * 0.01f;
                            newEdgeVisual.transform.localScale += -Vector3.up * Vector3.Distance(room.transform.TransformPoint(edge.From.LocalPosition), room.transform.TransformPoint(edge.To.LocalPosition)) * 0.5f;

                            newEdgeVisual.transform.position += Vector3.up * verticalOffset;

                            NetworkServer.Spawn(newEdgeVisual.gameObject);

                            edgeVisualArea = (newEdgeVisual, area);
                            EdgeVisuals.Add(edge, edgeVisualArea);
                        }

                        var (edgeVisual, _) = edgeVisualArea;

                        edgeVisual.NetworkMaterialColor = (NearestArea?.Edges.Contains(edge) ?? false) ? Color.yellow : Color.white;

                    }
                }
            }
            else
            {
                foreach (var (edgeVisual, area) in EdgeVisuals.Values)
                {
                    NetworkServer.Destroy(edgeVisual.gameObject);
                }
                EdgeVisuals.Clear();
            }
        }
    }
}
