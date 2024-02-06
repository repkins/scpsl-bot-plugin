using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class PickupKeycardContainmentOne : PickupItem
    {
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardContainmentOneWithinPickupDistance>();
        protected override ItemInInventory ItemInInventory => _botPlayer.MindRunner.GetBelief<KeycardContainmentOneInInventory>();

        public PickupKeycardContainmentOne(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
