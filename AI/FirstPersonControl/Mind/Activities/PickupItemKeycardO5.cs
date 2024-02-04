using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItemKeycardO5 : PickupItem
    {
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistanceKeycardO5>();
        protected override ItemInInventory ItemInInventory => _botPlayer.MindRunner.GetBelief<ItemInInventoryKeycardO5>();

        public PickupItemKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
