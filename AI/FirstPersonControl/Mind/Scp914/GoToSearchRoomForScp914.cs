﻿using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToSearchRoomForScp914 : IAction
    {
        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin>(this, b => b.Is(FacilityZone.LightContainment));
            roomEnterLocation = fpcMind.ActionEnabledBy<RoomEnterLocation>(this, b => b.Position.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<Scp914Location>(this);
        }

        private readonly FpcBotPlayer botPlayer;
        public GoToSearchRoomForScp914(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var enterPosition = roomEnterLocation.Position!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoomForScp914)}";
        }
    }
}
