using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard
{
    internal class PickupKeycard : PickupItemBase<KeycardWithPermissions>
    {
        protected override ItemWithinPickupDistance<KeycardWithPermissions> ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardWithinPickupDistance>(OfPermissions);
        protected override ItemInInventoryBase ItemInInventory => _botPlayer.MindRunner.GetBelief<ItemInInventory<KeycardWithPermissions>>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public PickupKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(ItemWithinPickupDistance<KeycardWithPermissions> b) => b.Criteria.Permissions == Permissions;
        private bool OfPermissions(ItemInInventory<KeycardWithPermissions> b) => b.Criteria.Permissions == Permissions;

        public override string ToString()
        {
            return $"{GetType().Name}({Permissions})";
        }
    }
}
