using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room
{
    internal class GoToZoneEnterLocation : IAction
    {
        public FacilityZone Zone { get; }
        public GoToZoneEnterLocation(FacilityZone zone, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            Zone = zone;
        }

        private ZoneEnterLocation zoneEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            zoneEnterLocation = fpcMind.ActionEnabledBy<ZoneEnterLocation>(this, b => b.Zone == Zone, b => b.Position.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(zoneEnterLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpactsWithCondition<ZoneWithin>(this, b => !b.HasTarget(Zone));
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
    }
}
