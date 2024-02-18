using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class FindItemOfType : FindItemBase<ItemOfType>
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
