using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class GoToPickupKeycard : GoToPickupItemBase
    {
        protected override ItemWithinSightBase          ItemWithinSight => _botPlayer.MindRunner.GetBelief<KeycardWithinSight>(OfPermissions);
        protected override ItemWithinPickupDistanceBase ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardWithinPickupDistance>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public GoToPickupKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(KeycardOfPermissions b) => b.Permissions == Permissions;
    }
}
