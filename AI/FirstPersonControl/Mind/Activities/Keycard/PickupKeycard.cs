using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class PickupKeycard : PickupItemBase
    {
        protected override ItemWithinPickupDistanceBase ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardWithinPickupDistance>(OfPermissions);
        protected override ItemInInventoryBase ItemInInventory => _botPlayer.MindRunner.GetBelief<KeycardInInventory>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public PickupKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(KeycardOfPermissions b) => b.Permissions == Permissions;
        private bool OfPermissions(KeycardInInventory b) => b.Permissions == Permissions;
    }
}
