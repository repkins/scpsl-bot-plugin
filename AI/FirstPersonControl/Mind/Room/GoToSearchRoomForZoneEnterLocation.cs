using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room
{
    internal class GoToSearchRoomForZoneEnterLocation : IAction
    {
        public FacilityZone TargetZone { get; }
        public FacilityZone ZoneFrom { get; }
        public GoToSearchRoomForZoneEnterLocation(FacilityZone targetZone, FacilityZone zoneFrom, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            TargetZone = targetZone;
            ZoneFrom = zoneFrom;
        }

        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin>(this, b => b.Is(ZoneFrom));
            roomEnterLocation = fpcMind.ActionEnabledBy<RoomEnterLocation>(this, b => b.Position.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ZoneEnterLocation>(this, b => b.Zone == TargetZone);
        }

        private readonly FpcBotPlayer botPlayer;
        private GoToSearchRoomForZoneEnterLocation(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Reset()
        { }

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
    }
}
