using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class FindKeycardContainmentOne : FindItem
    {
        protected override ItemWithinSight ItemWithinSight => botPlayer.MindRunner.GetBelief<KeycardContainmentOneWithinSight>();

        public FindKeycardContainmentOne(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
