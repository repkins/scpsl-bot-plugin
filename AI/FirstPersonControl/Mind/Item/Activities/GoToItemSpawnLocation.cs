using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToItemSpawnLocation<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToItemSpawnLocation(C criteria)
        {
            this.Criteria = criteria;
        }

        private ItemSpawnLocation<C> itemSpawnLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            itemSpawnLocation = fpcMind.ActivityEnabledBy<ItemSpawnLocation<C>>(this, b => b.Position.HasValue);

            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(itemSpawnLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
