using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using SCPSLBot.Navigation.Mesh;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.Navigation
{
    internal class NavigationSystem
    {
        public static NavigationSystem Instance { get; } = new NavigationSystem();

        public string BaseDir { get; set; }

        private NavigationMesh NavigationMesh { get; } = NavigationMesh.Instance;

        public void Init()
        {
            EventManager.RegisterEvents(this);

            LoadMesh();
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            Log.Info($"Initializing vertices and areas from room kind counterparts.");

            NavigationMesh.InitRoomVertices();
            NavigationMesh.InitRoomAreas();

            Timing.RunCoroutine(ConnectForeignAreasAsync());
        }

        private IEnumerator<float> ConnectForeignAreasAsync()
        {
            yield return Timing.WaitUntilTrue(() => SeedSynchronizer.MapGenerated);
            
            Log.Info($"Connecting areas between rooms.");
            foreach (var door in DoorVariant.AllDoors)
            {
                if (door.Rooms.Length == 2)
                {
                    var doorCenterPosition = door.transform.position + Vector3.up;  // assuming pivot point is located at the bottom of all doors

                    var edgeInFront = NavigationMesh.GetNearestEdge(doorCenterPosition, door.Rooms[0]);
                    var edgeInBack = NavigationMesh.GetNearestEdge(doorCenterPosition, door.Rooms[1]);
                    
                    if (edgeInFront != null && edgeInBack != null)
                    {
                        // Connect
                        var areaInFront = NavigationMesh.AreasByRoom[edgeInFront.Value.From.Room]
                            .Find(a => a.RoomKindArea.Edges.Any(e => e == new RoomKindEdge(edgeInFront.Value.From.RoomKindVertex, edgeInFront.Value.To.RoomKindVertex)));

                        var areaInBack = NavigationMesh.AreasByRoom[edgeInBack.Value.From.Room]
                            .Find(a => a.RoomKindArea.Edges.Any(e => e == new RoomKindEdge(edgeInBack.Value.From.RoomKindVertex, edgeInBack.Value.To.RoomKindVertex)));

                        areaInFront.ForeignConnectedAreas.Add(areaInBack);
                        areaInFront.ConnectedAreaEdges.Add(areaInBack, edgeInBack.Value);

                        areaInBack.ForeignConnectedAreas.Add(areaInFront);
                        areaInBack.ConnectedAreaEdges.Add(areaInFront, edgeInFront.Value);
                    }
                }
            }
            Log.Info($"Connecting areas between elevator destinations.");
            foreach (var (group, elevatorDoors) in ElevatorDoor.AllElevatorDoors)
            {
                if (elevatorDoors.Count != 2)
                {
                    Log.Warning($"Irregular elevator level count ({elevatorDoors.Count}) of group {group}");
                    continue;
                }

                var doorTransform = elevatorDoors[0].transform;
                var doorPosition = doorTransform.position;
                var doorForward = doorTransform.forward;
                var area0InShaft = NavigationMesh.GetAreaWithin(doorPosition - doorForward + Vector3.up);

                doorTransform = elevatorDoors[1].transform;
                doorPosition = doorTransform.position;
                doorForward = doorTransform.forward;
                var area1InShaft = NavigationMesh.GetAreaWithin(doorPosition - doorForward + Vector3.up);

                if (area0InShaft != null && area1InShaft != null)
                {
                    // Connect
                    area0InShaft.ForeignConnectedAreas.Add(area1InShaft);
                    area1InShaft.ForeignConnectedAreas.Add(area0InShaft);
                }
            }
            Log.Info($"Connecting areas finished.");
        }

        public void LoadMesh()
        {
            var fileName = "navmesh.slnmf";
            var path = Path.Combine(BaseDir, fileName);

            if (!File.Exists(path))
            {
                return;
            }

            using var fileStream = File.OpenRead(path);
            using var binaryReader = new BinaryReader(fileStream);

            NavigationMesh.ReadMesh(binaryReader);
        }

        public void SaveMesh()
        {
            var fileName = "navmesh.slnmf";
            var path = Path.Combine(BaseDir, fileName);

            using var fileStream = File.OpenWrite(path);
            using var binaryWriter = new BinaryWriter(fileStream);

            NavigationMesh.WriteMesh(binaryWriter);
        }        

        #region Private constructor
        private NavigationSystem()
        { }
        #endregion
    }
}
