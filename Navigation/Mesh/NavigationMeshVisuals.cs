using AdminToys;
using Mirror;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.Navigation.Mesh
{
    internal class NavigationMeshVisuals
    {
        public Player EnabledVisualsForPlayer { get; set; }

        public RoomKindArea NearestArea { get; set; }
        public RoomKindArea FacingAreaTemplate { get; set; }

        public List<Area> Path { get; } = new List<Area>();

        private Dictionary<RoomVertex, PrimitiveObjectToy> VertexVisuals { get; } = new();
        private Dictionary<(RoomKindVertex From, RoomKindVertex To), (PrimitiveObjectToy, Area)> EdgeVisuals { get; } = new();

        private Dictionary<Area, PrimitiveObjectToy> AreaVisuals { get; } = new ();
        private Dictionary<(Area From, Area To), PrimitiveObjectToy> AreaConnectionVisuals { get; } = new ();
        private Dictionary<(Area, Area), PrimitiveObjectToy> AreaConnectionOriginVisuals { get; } = new ();

        private NavigationMesh NavigationMesh { get; } = NavigationMesh.Instance;

        private RoomKindArea LastNearestArea { get; set; }
        private RoomKindArea LastFacingAreaTemplate { get; set; }

        private string[] AreaVisualsMessages { get; } = new string[2];

        public void UpdateAreaInfoVisuals()
        {
            if (EnabledVisualsForPlayer != null)
            {
                var nearestArea = NearestArea;

                if (nearestArea != LastNearestArea)
                {
                    LastNearestArea = nearestArea;

                    if (nearestArea != null)
                    {
                        //var connectedIdsStr = string.Join(", ", nearestArea.ConnectedAreas.Select(c => $"#{c.Id}"));
                        var nearestAreaId = NavigationMesh.AreasByRoomKind[nearestArea.RoomKind].IndexOf(nearestArea);
                        AreaVisualsMessages[0] = $"Area #{nearestAreaId} in {nearestArea.RoomKind}";
                    }
                    else
                    {
                        AreaVisualsMessages[0] = null;
                    }
                }

                if (FacingAreaTemplate != LastFacingAreaTemplate)
                {
                    LastFacingAreaTemplate = FacingAreaTemplate;

                    if (FacingAreaTemplate != null)
                    {
                        var facingAreaId = NavigationMesh.AreasByRoomKind[FacingAreaTemplate.RoomKind].IndexOf(FacingAreaTemplate);
                        AreaVisualsMessages[1] = $"Facing area #{facingAreaId} in {FacingAreaTemplate.RoomKind}";
                    }
                    else
                    {
                        AreaVisualsMessages[1] = null;
                    }
                }

                var messageLinesToSend = AreaVisualsMessages.Where(m => m != null);
                if (messageLinesToSend.Any())
                {
                    EnabledVisualsForPlayer.SendBroadcast(string.Join("\n", messageLinesToSend), 60, shouldClearPrevious: true);
                }
                else
                {
                    EnabledVisualsForPlayer.ClearBroadcasts();
                }
            }
        }

        public void UpdateAreaVisuals()
        {
            if (EnabledVisualsForPlayer != null)
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

                foreach (var ((from, to), visual) in AreaConnectionVisuals.Select(p => (p.Key, p.Value)).ToArray())
                {
                    var isAnyAreaRemoved = !AreaVisuals.ContainsKey(from) || !AreaVisuals.ContainsKey(to);

                    if (isAnyAreaRemoved || (!from.ConnectedAreas.Contains(to) && !to.ConnectedAreas.Contains(from)))
                    {
                        NetworkServer.Destroy(visual.gameObject);
                        AreaConnectionVisuals.Remove((from, to));
                    }

                    if ((isAnyAreaRemoved || !from.ConnectedAreas.Contains(to)) && AreaConnectionOriginVisuals.TryGetValue((from, to), out var originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        AreaConnectionOriginVisuals.Remove((from, to));
                    }

                    if ((isAnyAreaRemoved || !to.ConnectedAreas.Contains(from)) && AreaConnectionOriginVisuals.TryGetValue((to, from), out originVisual))
                    {
                        NetworkServer.Destroy(originVisual.gameObject);
                        AreaConnectionOriginVisuals.Remove((to, from));
                    }
                }

                foreach (var area in NavigationMesh.AreasByRoom.Values.SelectMany(l => l))
                {
                    var room = area.Room.Identifier;

                    if (!AreaVisuals.TryGetValue(area, out var visual))
                    {
                        visual = UnityEngine.Object.Instantiate(primPrefab);
                        NetworkServer.Spawn(visual.gameObject);

                        visual.transform.position = room.transform.TransformPoint(area.LocalCenterPosition);
                        visual.transform.localScale = -Vector3.one * 0.25f;

                        AreaVisuals.Add(area, visual);
                    }

                    visual.NetworkMaterialColor = (area.RoomKindArea == FacingAreaTemplate) ? Color.green : Color.yellow;

                    foreach (var connectedArea in area.ConnectedAreas)
                    {
                        var connectedRoom = connectedArea.Room.Identifier;
                        if (!AreaConnectionVisuals.TryGetValue((area, connectedArea), out var outConnectionVisual))
                        {
                            if (!AreaConnectionVisuals.TryGetValue((connectedArea, area), out var inConnectionVisual))
                            {
                                outConnectionVisual = UnityEngine.Object.Instantiate(primPrefab);
                                outConnectionVisual.NetworkPrimitiveType = PrimitiveType.Cylinder;
                                outConnectionVisual.transform.position = Vector3.Lerp(room.transform.TransformPoint(area.LocalCenterPosition), connectedRoom.transform.TransformPoint(connectedArea.LocalCenterPosition), 0.5f);
                                outConnectionVisual.transform.LookAt(connectedRoom.transform.TransformPoint(connectedArea.LocalCenterPosition));
                                outConnectionVisual.transform.RotateAround(outConnectionVisual.transform.position, outConnectionVisual.transform.right, 90f);
                                outConnectionVisual.transform.localScale = -Vector3.forward * 0.1f + -Vector3.right * 0.1f;
                                outConnectionVisual.transform.localScale += -Vector3.up * Vector3.Distance(room.transform.TransformPoint(area.LocalCenterPosition), connectedRoom.transform.TransformPoint(connectedArea.LocalCenterPosition)) * 0.5f;
                                NetworkServer.Spawn(outConnectionVisual.gameObject);

                                AreaConnectionVisuals.Add((area, connectedArea), outConnectionVisual);
                            }
                            else
                            {
                                outConnectionVisual = inConnectionVisual;
                            }
                        }

                        if (!AreaConnectionOriginVisuals.TryGetValue((area, connectedArea), out var outConnectionOriginVisual))
                        {
                            outConnectionOriginVisual = UnityEngine.Object.Instantiate(primPrefab);
                            outConnectionOriginVisual.transform.position = room.transform.TransformPoint(area.LocalCenterPosition);
                            outConnectionOriginVisual.transform.position += Vector3.Normalize(connectedRoom.transform.TransformPoint(connectedArea.LocalCenterPosition) - outConnectionOriginVisual.transform.position) * 0.125f;
                            outConnectionOriginVisual.transform.localScale = -Vector3.one * 0.2f;
                            NetworkServer.Spawn(outConnectionOriginVisual.gameObject);

                            AreaConnectionOriginVisuals.Add((area, connectedArea), outConnectionOriginVisual);
                        }

                        if (AreaConnectionOriginVisuals.TryGetValue((connectedArea, area), out var inConnectionOriginVisual))
                        {
                            outConnectionVisual.NetworkMaterialColor = Color.yellow;
                            outConnectionOriginVisual.NetworkMaterialColor = Color.yellow;
                            inConnectionOriginVisual.NetworkMaterialColor = Color.yellow;
                        }
                        else
                        {
                            outConnectionVisual.NetworkMaterialColor = Color.white;
                            outConnectionOriginVisual.NetworkMaterialColor = Color.white;
                        }
                    }
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

                foreach (var connectionVisual in AreaConnectionVisuals.Values)
                {
                    NetworkServer.Destroy(connectionVisual.gameObject);
                }
                AreaConnectionVisuals.Clear();

                foreach (var connectionOriginVisual in AreaConnectionOriginVisuals.Values)
                {
                    NetworkServer.Destroy(connectionOriginVisual.gameObject);
                }
                AreaConnectionOriginVisuals.Clear();
            }
        }

        public void UpdateVertexVisuals()
        {
            if (EnabledVisualsForPlayer != null)
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

                        VertexVisuals.Add(vertex, visual);
                    }

                    //visual.NetworkMaterialColor = (vertex.RoomKindVertex == FacingVertexTemplate) ? Color.green : Color.yellow;

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
            if (EnabledVisualsForPlayer != null)
            {
                foreach (var (edge, (visual, area)) in EdgeVisuals.Select(p => (p.Key, p.Value)))
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
                            var (edgeVisual,_) = edgeVisualArea;

                            edgeVisual = UnityEngine.Object.Instantiate(primPrefab);
                            edgeVisual.NetworkPrimitiveType = PrimitiveType.Cylinder;
                            edgeVisual.transform.position = Vector3.Lerp(room.transform.TransformPoint(edge.From.LocalPosition), room.transform.TransformPoint(edge.To.LocalPosition), 0.5f);
                            edgeVisual.transform.LookAt(room.transform.TransformPoint(edge.To.LocalPosition));
                            edgeVisual.transform.RotateAround(edgeVisual.transform.position, edgeVisual.transform.right, 90f);
                            edgeVisual.transform.localScale = -Vector3.forward * 0.01f + -Vector3.right * 0.01f;
                            edgeVisual.transform.localScale += -Vector3.up * Vector3.Distance(room.transform.TransformPoint(edge.From.LocalPosition), room.transform.TransformPoint(edge.To.LocalPosition)) * 0.5f;
                            NetworkServer.Spawn(edgeVisual.gameObject);

                            EdgeVisuals.Add(edge, (edgeVisual, area));
                        }
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
