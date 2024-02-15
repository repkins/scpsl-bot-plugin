using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class GoToPickupItem : GoToPickupItemBase, IActivity
    {
        public ItemType ItemType { get; }
        public GoToPickupItem(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            ItemType = itemType;
        }

        protected override ItemWithinSightBase ItemWithinSight => _botPlayer.MindRunner.GetBelief<ItemWithinSight>(OfItemType);
        protected override ItemWithinPickupDistanceBase ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistance>(OfItemType);

        private bool OfItemType(ItemWithinSight b) => b.ItemType == this.ItemType;
        private bool OfItemType(ItemWithinPickupDistance b) => b.ItemType == this.ItemType;

    }
}
