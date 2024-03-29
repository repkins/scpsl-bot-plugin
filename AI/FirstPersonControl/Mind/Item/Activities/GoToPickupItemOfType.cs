using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToPickupItemOfType : GoToPickupItemBase<ItemOfType>
    {
        public ItemType ItemType { get; }
        public GoToPickupItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            ItemType = itemType;
        }

        protected override ItemWithinSight<ItemOfType> ItemWithinSight => _botPlayer.MindRunner.GetBelief<ItemOfTypeWithinSight>(OfItemType);
        protected override ItemWithinPickupDistance<ItemOfType> ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemOfTypeWithinPickupDistance>(OfItemType);

        private bool OfItemType(ItemPickup<ItemPickupBase, ItemOfType> b) => b.Criteria.ItemType == ItemType;

        public override string ToString()
        {
            return $"{GetType().Name}({ItemType})";
        }
    }
}
