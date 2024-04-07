using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToNextRoomForItem<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToNextRoomForItem(C criteria)
        {
            this.Criteria = criteria;
        }

        private NextRoomLocationForItem<C> nextRoomLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            nextRoomLocation = fpcMind.ActivityEnabledBy<NextRoomLocationForItem<C>>(this, b => b.Criteria.Equals(Criteria), b => b.Position.HasValue);
            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(nextRoomLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemLocation<C>>(this, b => b.Criteria.Equals(Criteria));
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
