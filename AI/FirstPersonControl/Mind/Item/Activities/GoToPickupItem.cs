using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal abstract class GoToPickupItem<C> : IActivity where C : IItemBeliefCriteria
    {
        protected abstract ItemWithinSight<C> ItemWithinSight { get; }
        protected abstract ItemWithinPickupDistance<C> ItemWithinPickupDistance { get; }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy(this, ItemWithinSight, b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts(this, ItemWithinPickupDistance);
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
