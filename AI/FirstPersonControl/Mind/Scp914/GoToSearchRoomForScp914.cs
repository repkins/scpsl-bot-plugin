using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToSearchRoomForScp914 : GoTo<RoomEnterLocation>
    {
        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin, FacilityZone?>(this, b => FacilityZone.LightContainment, b => b.Zone);
            
            base.SetEnabledByBeliefs(fpcMind);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<Scp914Location>(this);
        }

        private readonly FpcBotPlayer botPlayer;
        public GoToSearchRoomForScp914(FpcBotPlayer botPlayer) : base(0)
        {
            this.botPlayer = botPlayer;
        }

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
            return $"{nameof(GoToSearchRoomForScp914)}";
        }
    }
}
