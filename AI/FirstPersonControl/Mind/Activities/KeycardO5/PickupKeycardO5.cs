using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.KeycardO5
{
    internal class PickupKeycardO5 : PickupItem
    {
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardO5WithinPickupDistance>();
        protected override ItemInInventory ItemInInventory => _botPlayer.MindRunner.GetBelief<KeycardO5InInventory>();

        public PickupKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
