using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class FindItem : FindItemBase
    {
        public readonly ItemType ItemType;
        public FindItem(ItemType itemType, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.ItemType = itemType;
        }

        protected override ItemWithinSightBase ItemWithinSight => botPlayer.MindRunner.GetBelief<ItemWithinSight>(OfItemType);

        private bool OfItemType(ItemOfType b) => b.ItemType == this.ItemType;
    }
}
