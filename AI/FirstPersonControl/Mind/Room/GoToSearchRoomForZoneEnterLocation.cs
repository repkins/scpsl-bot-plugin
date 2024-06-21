using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room
{
    internal class GoToSearchRoomForZoneEnterLocation : GoTo<RoomEnterLocation>
    {
        public FacilityZone TargetZone { get; }
        public FacilityZone ZoneFrom { get; }
        public GoToSearchRoomForZoneEnterLocation(FacilityZone targetZone, FacilityZone zoneFrom, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            TargetZone = targetZone;
            ZoneFrom = zoneFrom;
        }

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin, FacilityZone?>(this, b => ZoneFrom, b => b.Zone);

            base.SetEnabledByBeliefs(fpcMind);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ZoneEnterLocation>(this, b => b.Zone == TargetZone && b.FromZone == ZoneFrom);
        }

        private readonly FpcBotPlayer botPlayer;
        private GoToSearchRoomForZoneEnterLocation(FpcBotPlayer botPlayer) : base(0)
        {
            this.botPlayer = botPlayer;
        }

        public override void Reset()
        { }

        public override void Tick()
        {
            var enterPosition = location.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoomForZoneEnterLocation)}({TargetZone} <- {ZoneFrom})";
        }
    }
}
