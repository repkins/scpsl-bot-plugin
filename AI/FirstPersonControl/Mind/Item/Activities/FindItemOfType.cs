namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class FindItemOfType : FindItem<ItemOfType>
    {
        public FindItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(new(itemType), botPlayer)
        { }
    }
}
