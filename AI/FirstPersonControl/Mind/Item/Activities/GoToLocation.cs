using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal abstract class GoToLocation<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        protected ItemLocation<C> itemLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
            fpcMind.ActivityEnabledBy<GlassObstacle>(this, b => !b.Is(this.itemLocation.AccessiblePosition!.Value));
        }

        public abstract void SetImpactsBeliefs(FpcMind fpcMind);

        public abstract void Reset();

        public abstract void Tick();
    }
}
