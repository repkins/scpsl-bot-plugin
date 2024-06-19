using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room
{
    internal class GoToZoneEnterLocation : IAction
    {
        public FacilityZone Zone { get; }
        public FacilityZone FromZone { get; }
        public GoToZoneEnterLocation(FacilityZone zone, FacilityZone fromZone, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            Zone = zone;
            FromZone = fromZone;
        }

        private ZoneEnterLocation zoneEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            zoneEnterLocation = fpcMind.ActionEnabledBy<ZoneEnterLocation>(this, b => b.Zone == Zone && b.FromZone == FromZone, b => b.Position.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(zoneEnterLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ZoneWithin, FacilityZone?>(this, b => Zone);
        }

        public void Reset()
        { }

        private readonly FpcBotPlayer botPlayer;
        private GoToZoneEnterLocation(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var enterPosition = zoneEnterLocation.Position!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GoToZoneEnterLocation)}({Zone} from {FromZone})";
        }
    }
}
