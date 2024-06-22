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

        private readonly FpcBotPlayer botPlayer;
        private GoToSearchRoomForZoneEnterLocation(FpcBotPlayer botPlayer) : base(0, botPlayer)
        {
            this.botPlayer = botPlayer;
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

        public override float Weight { get; } = 10f / 2; // num of target zone enter locations over num of zone within rooms, 2/10 = 1/5
        public override float Cost => Weight * 10;

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

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoomForZoneEnterLocation)}({TargetZone} <- {ZoneFrom})";
        }
    }
}
