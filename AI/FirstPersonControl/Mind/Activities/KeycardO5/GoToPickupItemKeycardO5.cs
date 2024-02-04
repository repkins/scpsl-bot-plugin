using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.KeycardO5;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.KeycardO5
{
    internal class GoToPickupItemKeycardO5 : GoToPickupItem
    {
        protected override ItemWithinSight ItemWithinSight => _botPlayer.MindRunner.GetBelief<ItemWithinSightKeycardO5>();
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistanceKeycardO5>();

        public GoToPickupItemKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
