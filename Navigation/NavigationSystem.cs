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
            Log.Info($"Initializing vertices and areas from room kind counterparts.");

            NavigationMesh.InitRoomVertices();
            NavigationMesh.InitRoomAreas();

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

                    var edgeInFront = NavigationMesh.GetNearestEdge(doorPosition + doorForward * .75f);
                    var edgeInBack = NavigationMesh.GetNearestEdge(doorPosition - doorForward * .75f);

                    if (edgeInFront != null && edgeInBack != null)
                    {
                        // Connect
                        var areaInFront = NavigationMesh.AreasByRoom[edgeInFront.Value.From.Room]
                            .Find(a => a.RoomKindArea.Edges.Any(e => e == (edgeInFront.Value.From.RoomKindVertex, edgeInFront.Value.To.RoomKindVertex)));

                        var areaInBack = NavigationMesh.AreasByRoom[edgeInBack.Value.From.Room]
                            .Find(a => a.RoomKindArea.Edges.Any(e => e == (edgeInBack.Value.From.RoomKindVertex, edgeInBack.Value.To.RoomKindVertex)));

                        areaInFront.ForeignConnectedAreas.Add(areaInBack);
                        areaInBack.ForeignConnectedAreas.Add(areaInFront);

                        areaInFront.ConnectedAreaEdges.Add(areaInBack, edgeInFront.Value);
                        areaInBack.ConnectedAreaEdges.Add(areaInFront, edgeInBack.Value);
                    }
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
