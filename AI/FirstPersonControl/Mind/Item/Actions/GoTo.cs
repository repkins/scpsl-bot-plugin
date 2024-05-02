using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal abstract class GoTo<B, C> : IAction where B : ItemLocation<C> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        protected GoTo(C criteria)
        {
            this.Criteria = criteria;
        }

        protected ItemLocation<C> itemLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemLocation = fpcMind.ActionEnabledBy<B>(this, b => b.Criteria.Equals(Criteria), b => b.AccessiblePosition.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
            fpcMind.ActionEnabledBy<GlassObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
        }

        public abstract void SetImpactsBeliefs(FpcMind fpcMind);

        public abstract void Reset();

        public abstract void Tick();
    }
}
