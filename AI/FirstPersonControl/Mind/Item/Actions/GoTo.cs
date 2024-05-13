using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal abstract class GoTo<TItemLocation, TCriteria> : IAction 
        where TItemLocation : ItemLocation<TCriteria> 
        where TCriteria : IItemBeliefCriteria, IEquatable<TCriteria>
    {
        public readonly TCriteria Criteria;
        protected GoTo(TCriteria criteria)
        {
            this.Criteria = criteria;
        }

        protected TItemLocation itemLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemLocation = fpcMind.ActionEnabledBy<TItemLocation>(this, b => b.Criteria.Equals(Criteria), b => b.AccessiblePosition.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
            fpcMind.ActionEnabledBy<GlassObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
        }

        public abstract void SetImpactsBeliefs(FpcMind fpcMind);

        public abstract void Reset();

        public abstract void Tick();
    }
}
