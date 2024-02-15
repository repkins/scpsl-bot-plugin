namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinSight : ItemWithinSightBase
    {
        public ItemType ItemType { get; }
        public ItemWithinSight(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}
