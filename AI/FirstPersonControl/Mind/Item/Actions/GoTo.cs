using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal abstract class GoTo<TItemLocation, TCriteria> : GoTo<TItemLocation>
        where TItemLocation : ItemLocations<TCriteria>
        where TCriteria : IItemBeliefCriteria, IEquatable<TCriteria>
    {
        public readonly TCriteria Criteria;
        protected GoTo(TCriteria criteria, int idx) : base(idx)
        {
            this.Criteria = criteria;
        }

        protected override TItemLocation SetEnabledByLocation(FpcMind fpcMind, Func<TItemLocation, bool> currentGetter)
        {
            return fpcMind.ActionEnabledBy<TItemLocation>(this, b => b.Criteria.Equals(Criteria), currentGetter);
        }
    }
}
