using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class ItemActivity
    {
        public readonly ItemType ItemType;
        public ItemActivity(ItemType itemType)
        {
            this.ItemType = itemType;
        }

        protected bool OfItemType(ItemBase b) => b.ItemType == this.ItemType;
    }
}
