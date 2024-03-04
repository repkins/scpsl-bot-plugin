﻿using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItemOfType : PickupItemBase<ItemOfType>
    {
        protected override ItemWithinPickupDistance<ItemOfType> ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemOfTypeWithinPickupDistance>(OfItemType);
        protected override ItemInInventoryBase ItemInInventory => _botPlayer.MindRunner.GetBelief<ItemInInventory<ItemOfType>>(OfItemType);

        public ItemType ItemType { get; }
        public PickupItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.ItemType = itemType;
        }

        private bool OfItemType(ItemWithinPickupDistance<ItemOfType> b) => b.Criteria.ItemType == this.ItemType;
        private bool OfItemType(ItemInInventory<ItemOfType> b) => b.Criteria.ItemType == this.ItemType;

        public override string ToString()
        {
            return $"{GetType().Name}({ItemType})";
        }
    }
}
