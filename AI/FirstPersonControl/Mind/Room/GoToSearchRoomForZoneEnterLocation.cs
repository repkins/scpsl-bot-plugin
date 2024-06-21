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
        public int Idx { get; }
        public GoToSearchRoomForZoneEnterLocation(FacilityZone targetZone, FacilityZone zoneFrom, int idx, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            TargetZone = targetZone;
            ZoneFrom = zoneFrom;
            Idx = idx;
        }

        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin, FacilityZone?>(this, b => ZoneFrom, b => b.Zone);
            roomEnterLocation = fpcMind.ActionEnabledBy<RoomEnterLocation>(this, b => b.Positions.Count > Idx);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Positions[Idx]));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ZoneEnterLocation>(this, b => b.Zone == TargetZone && b.FromZone == ZoneFrom);
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
            var enterPosition = roomEnterLocation.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoomForZoneEnterLocation)}({TargetZone} <- {ZoneFrom}, {Idx})";
        }
    }
}
