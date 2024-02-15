using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItem : PickupItemBase
    {
        protected override ItemWithinPickupDistanceBase ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistance>(OfItemType);
        protected override ItemInInventoryBase ItemInInventory => _botPlayer.MindRunner.GetBelief<ItemInInventory>(OfItemType);

        public ItemType ItemType { get; }
        public PickupItem(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.ItemType = itemType;
        }

        private bool OfItemType(ItemWithinPickupDistance b) => b.ItemType == this.ItemType;
        private bool OfItemType(ItemInInventory b) => b.ItemType == this.ItemType;
    }
}
