namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class PickupItemOfType : PickupItem<ItemOfType>
    {
        public PickupItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(new(itemType), botPlayer)
        {
        }
    }
}
