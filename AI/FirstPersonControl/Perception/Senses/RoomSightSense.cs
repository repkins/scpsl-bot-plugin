using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class RoomSightSense : SightSense
    {
        public IEnumerable<Area> ForeignRoomsAreas { get; private set; }
        public RoomIdentifier RoomWithin { get; private set; }

        public event Action<Area> OnSensedForeignRoomArea;
        public event Action OnAfterSensedForeignRooms;

        public event Action<RoomIdentifier> OnSensedRoomWithin;

        private readonly FpcBotPlayer _fpcBotPlayer;

        public RoomSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public override void ProcessSensibility(Collider collider)
        { }

        public override void Reset()
        { }

        public override void ProcessSensedItems()
        {
            base.ProcessSensedItems();

            var playerPosition = _fpcBotPlayer.FpcRole.transform.position;
            var playerForward = _fpcBotPlayer.FpcRole.transform.forward;

            var newRoomWithin = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (newRoomWithin is null)
            {
                Log.Debug($"Could not determine room bot currently in");
                return;
            }

            if (newRoomWithin != RoomWithin)
            {
                OnSensedRoomWithin?.Invoke(newRoomWithin);
            }
            RoomWithin = newRoomWithin;

            ForeignRoomsAreas = NavigationMesh.Instance.AreasByRoom[RoomWithin.ApiRoom]
                .Where(a => a.ForeignConnectedAreas.Any())
                .SelectMany(a => a.ForeignConnectedAreas)
                .Select(fa => fa.ConnectedAreas.First());

            foreach (var sensedForeignRoomArea in ForeignRoomsAreas)
            {
                OnSensedForeignRoomArea?.Invoke(sensedForeignRoomArea);
            }
            OnAfterSensedForeignRooms?.Invoke();
        }
    }
}
