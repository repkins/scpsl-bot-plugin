﻿using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class FindItemOfType : FindItem<ItemOfType>
    {
        public readonly ItemType ItemType;
        public FindItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.ItemType = itemType;
        }

        protected override ItemWithinSight<ItemOfType> ItemWithinSight => botPlayer.MindRunner.GetBelief<ItemOfTypeWithinSight>(OfItemType);

        private bool OfItemType(ItemWithinSight<ItemOfType> b) => b.Criteria.ItemType == this.ItemType;

        public override string ToString()
        {
            return $"{GetType().Name}({ItemType})";
        }
    }
}
