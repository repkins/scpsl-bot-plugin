namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToPickupItemOfType : GoToPickupItem<ItemOfType>
    {
        public GoToPickupItemOfType(ItemType itemType, FpcBotPlayer botPlayer) : base(new(itemType), botPlayer)
        {
        }
    }
}
