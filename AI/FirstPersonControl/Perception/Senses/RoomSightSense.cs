using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class RoomSightSense : SightSense
    {
        public List<Area> ForeignRoomsAreas { get; } = new();
        public RoomIdentifier RoomWithin { get; private set; }

        public event Action<Area> OnSensedForeignRoomArea;
        public event Action OnAfterSensedForeignRooms;

        public event Action<RoomIdentifier> OnSensedRoomWithin;

        private readonly FpcBotPlayer _fpcBotPlayer;

        public RoomSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        { yield break; }

        public override void Reset()
        { }

        public override void ProcessSightSensedItems()
        {
            UpdateRoomWithin();
            UpdateForeignRoomsAreas();

            foreach (var sensedForeignRoomArea in ForeignRoomsAreas)
            {
                OnSensedForeignRoomArea?.Invoke(sensedForeignRoomArea);
            }
            OnAfterSensedForeignRooms?.Invoke();
        }

        private void UpdateRoomWithin()
        {
            var playerPosition = _fpcBotPlayer.PlayerPosition;

            var newRoomWithin = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (newRoomWithin is null)
            {
                Log.Warning($"Could not determine room bot currently in");
                return;
            }

            if (newRoomWithin != RoomWithin)
            {
                OnSensedRoomWithin?.Invoke(newRoomWithin);
            }
            RoomWithin = newRoomWithin;
        }

        private void UpdateForeignRoomsAreas()
        {
            ForeignRoomsAreas.Clear();
            foreach (var a in NavigationMesh.Instance.AreasByRoom[RoomWithin.ApiRoom])
            {
                if (a.ForeignConnectedAreas.Count > 0)
                {
                    foreach (var fa in a.ForeignConnectedAreas)
                    {
                        var faa = fa.ConnectedAreas.First();
                        ForeignRoomsAreas.Add(faa);
                    }
                }
            }
        }
    }
}
