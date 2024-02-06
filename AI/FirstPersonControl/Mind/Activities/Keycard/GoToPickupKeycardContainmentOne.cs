using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class GoToPickupKeycardContainmentOne : GoToPickupItem
    {
        protected override ItemWithinSight ItemWithinSight => _botPlayer.MindRunner.GetBelief<KeycardContainmentOneWithinSight>();
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardContainmentOneWithinPickupDistance>();

        public GoToPickupKeycardContainmentOne(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
