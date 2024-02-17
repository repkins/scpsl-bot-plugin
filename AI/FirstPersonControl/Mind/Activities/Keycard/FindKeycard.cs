using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class FindKeycard : FindItemBase
    {
        protected override ItemWithinSightBase ItemWithinSight => botPlayer.MindRunner.GetBelief<KeycardWithinSight>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public FindKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(KeycardOfPermissions obj) => obj.Permissions == Permissions;
    }
}
