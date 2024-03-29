using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class PickupItemOfType : PickupItemBase<ItemOfType>
    {
        protected override ItemWithinPickupDistance<ItemOfType> ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemOfTypeWithinPickupDistance>(OfItemType);
        protected override ItemInInventoryBase ItemInInventory => _botPlayer.MindRunner.GetBelief<ItemInInventory<ItemOfType>>(OfItemType);

        public ItemType ItemType { get; }
        public PickupItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            ItemType = itemType;
        }

        private bool OfItemType(ItemWithinPickupDistance<ItemOfType> b) => b.Criteria.ItemType == ItemType;
        private bool OfItemType(ItemInInventory<ItemOfType> b) => b.Criteria.ItemType == ItemType;

        public override string ToString()
        {
            return $"{GetType().Name}({ItemType})";
        }
    }
}
