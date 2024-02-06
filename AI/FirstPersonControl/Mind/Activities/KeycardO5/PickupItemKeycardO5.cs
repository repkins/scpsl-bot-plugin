using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.KeycardO5
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
