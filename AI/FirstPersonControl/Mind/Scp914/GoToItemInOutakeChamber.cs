using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToItemInOutakeChamber<C> : GoTo<ItemsInOutakeChamber, IItemBeliefCriteria> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public GoToItemInOutakeChamber(C criteria, FpcBotPlayer botPlayer) : base(criteria, 0, botPlayer)
        { 
            this.botPlayer = botPlayer;
        }

        private Scp914Location scp914Location;

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.scp914Location = fpcMind.GetBelief<Scp914Location>();

            base.SetEnabledByBeliefs(fpcMind, () => scp914Location.OutakeChamberPosition!.Value);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocations<C>>(this, b => b.Criteria.Equals(this.Criteria));
        }

        public override float Weight { get; } = 0f;

        private readonly FpcBotPlayer botPlayer;

        public override void Tick()
        {
            var outakeChamberPosition = scp914Location.OutakeChamberPosition!.Value;
            var playerPosition = botPlayer.PlayerPosition with { y = outakeChamberPosition.y };

            botPlayer.MoveToPosition(outakeChamberPosition);

            var transformedItemPosition = location.Positions[Idx];
            botPlayer.LookToPosition(transformedItemPosition);
        }

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToItemInOutakeChamber<C>)}({this.Criteria})";
        }
    }
}
