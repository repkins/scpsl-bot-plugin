using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class FindKeycardContainmentOne : FindItemBase
    {
        protected override ItemWithinSightBase ItemWithinSight => botPlayer.MindRunner.GetBelief<KeycardContainmentOneWithinSight>();

        public FindKeycardContainmentOne(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
