using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class RoomSightSense : SightSense, ISense
    {
        public IEnumerable<Area> ForeignRoomsAreas { get; private set; }
        public RoomIdentifier RoomWithin { get; private set; }

        public event Action<Area> OnSensedForeignRoomArea;
        public event Action OnAfterSensedForeignRooms;

        private readonly FpcBotPlayer _fpcBotPlayer;

        public RoomSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void ProcessSensibility(Collider collider)
        { }

        public void Reset()
        { }

        public void ProcessSensedItems()
        {
            var playerPosition = _fpcBotPlayer.FpcRole.transform.position;
            var playerForward = _fpcBotPlayer.FpcRole.transform.forward;

            RoomWithin = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (RoomWithin is null)
            {
                Log.Debug($"Could not determine room bot currently in");
                return;
            }

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
