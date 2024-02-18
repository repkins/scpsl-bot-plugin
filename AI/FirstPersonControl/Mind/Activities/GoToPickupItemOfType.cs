using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
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

        private bool OfItemType(ItemPickup<ItemPickupBase, ItemOfType> b) => b.Criteria.ItemType == this.ItemType;

    }
}
