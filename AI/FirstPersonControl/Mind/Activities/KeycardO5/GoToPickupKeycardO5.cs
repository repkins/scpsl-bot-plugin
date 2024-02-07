using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.KeycardO5
{
    internal class GoToPickupKeycardO5 : GoToPickupItem
    {
        protected override ItemWithinSight ItemWithinSight => _botPlayer.MindRunner.GetBelief<KeycardO5WithinSight>();
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardO5WithinPickupDistance>();

        public GoToPickupKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
