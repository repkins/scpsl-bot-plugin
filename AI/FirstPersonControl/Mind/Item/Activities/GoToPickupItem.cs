using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToPickupItem<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToPickupItem(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight<C>>(this, b => b.Criteria.Equals(Criteria), b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinPickupDistance<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public GoToPickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var targetItemPosition = _itemWithinSight.Item.transform.position;

            _botPlayer.MoveToPosition(targetItemPosition);
        }

        public void Reset() { }

        private ItemWithinSight<C> _itemWithinSight;
        protected readonly FpcBotPlayer _botPlayer;
    }
}
