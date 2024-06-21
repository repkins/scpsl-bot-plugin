using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room
{
    internal class GoToZoneEnterLocation : GoTo<ZoneEnterLocation>
    {
        public FacilityZone Zone { get; }
        public FacilityZone FromZone { get; }
        public GoToZoneEnterLocation(FacilityZone zone, FacilityZone fromZone, FpcBotPlayer botPlayer) : base(0)
        {
            Zone = zone;
            FromZone = fromZone;

            this.botPlayer = botPlayer;
        }

        protected override ZoneEnterLocation SetEnabledByLocation(FpcMind fpcMind, Func<ZoneEnterLocation, bool> currentGetter)
        {
            return fpcMind.ActionEnabledBy<ZoneEnterLocation>(this, b => b.Zone == Zone && b.FromZone == FromZone, currentGetter);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ZoneWithin, FacilityZone?>(this, b => Zone);
        }

        private readonly FpcBotPlayer botPlayer;

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
            return $"{nameof(GoToZoneEnterLocation)}({Zone} from {FromZone})";
        }
    }
}
