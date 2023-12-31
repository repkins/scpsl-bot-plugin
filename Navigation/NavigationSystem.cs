using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            Log.Info($"Initializing vertices from room kind vertices.");

            InitRoomVertices();
            InitRoomAreas();

            Timing.RunCoroutine(ConnectForeignAreasAsync());
        }

        private IEnumerator<float> ConnectForeignAreasAsync()
        {
            yield return Timing.WaitUntilTrue(() => SeedSynchronizer.MapGenerated);

            Log.Info($"Connecting areas between rooms.");
            foreach (var door in Facility.Doors)
            {
                if (door.OriginalObject.Rooms.Length == 2)
                {
                    var doorPosition = door.Position;
                    var doorForward = door.Transform.forward;

                    var areaInFront = NavigationMesh.GetAreaWithin(doorPosition + doorForward * 1f);
                    var areaInBack = NavigationMesh.GetAreaWithin(doorPosition - doorForward * 1f);

                    if (areaInFront != null && areaInBack != null)
                    {
                        // Connect
                        areaInFront.ForeignConnectedAreas.Add(areaInBack);
                        areaInBack.ForeignConnectedAreas.Add(areaInFront);
                    }
                }
            }
            Log.Info($"Connecting areas finished.");
        }

        public void LoadMesh()
        {
            var fileName = "navmesh.slnmf";
            var path = Path.Combine(BaseDir, fileName);

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

        public void InitRoomVertices()
        {
            foreach (var room in Facility.Rooms)
            {
                var vertices = new List<RoomVertex>();
                NavigationMesh.VerticesByRoom.Add(room, vertices);

                if (!NavigationMesh.VerticesByRoomKind.TryGetValue((room.Identifier.Name, room.Identifier.Shape, (RoomZone)room.Identifier.Zone), out var roomKindVertices))
                {
                    continue;
                }

                vertices.AddRange(roomKindVertices.Select(k => new RoomVertex(k, room)));
            }
        }

        public void ResetVertices()
        {
            NavigationMesh.VerticesByRoom.Clear();
        }

        public void InitRoomAreas()
        {
            foreach (var room in Facility.Rooms)
            {
                var areas = new List<Area>();
                NavigationMesh.AreasByRoom.Add(room, areas);

                if (!NavigationMesh.AreasByRoomKind.TryGetValue((room.Identifier.Name, room.Identifier.Shape, (RoomZone)room.Identifier.Zone), out var roomKindAreas))
                {
                    continue;
                }

                areas.AddRange(roomKindAreas.Select(k => new Area(k, room)));
            }
        }

        public void ResetAreas()
        {
            NavigationMesh.AreasByRoom.Clear();
        }

        #region Private constructor
        private NavigationSystem()
        { }
        #endregion
    }
}
