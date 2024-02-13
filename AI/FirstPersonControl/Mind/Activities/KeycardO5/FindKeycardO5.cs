using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class FindKeycardO5 : FindItem
    {
        protected override ItemWithinSight ItemWithinSight => botPlayer.MindRunner.GetBelief<KeycardO5WithinSight>();

        public FindKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
