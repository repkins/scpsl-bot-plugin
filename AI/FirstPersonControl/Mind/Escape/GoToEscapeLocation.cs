using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Escape
{
    internal class GoToEscapeLocation : GoTo<FacilityEscapeLocation>
    {
        private readonly FpcBotPlayer botPlayer;

        public GoToEscapeLocation(FpcBotPlayer botPlayer) : base(0, botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin, FacilityZone?>(this, b => FacilityZone.Surface, b => b.Zone);

            base.SetEnabledByBeliefs(fpcMind);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<PlayerEscaped, bool>(this, b => true);
        }

        public override float Weight { get; } = 1f;

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
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToEscapeLocation)}";
        }
    }
}
