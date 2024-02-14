using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class GoToPickupItem : ItemActivity, IActivity
    {
        public GoToPickupItem(ItemType itemType, FpcBotPlayer botPlayer) : base(itemType)
        {
            this._botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSight>(this, OfItemType, b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinPickupDistance>(this, OfItemType);
        }

        public void Reset() { }

        public void Tick()
        {
            var targetItemPosition = _itemWithinSight.Item.transform.position;

            _botPlayer.MoveToPosition(targetItemPosition);
        }

        private ItemWithinSight _itemWithinSight;
        protected readonly FpcBotPlayer _botPlayer;
    }
}
