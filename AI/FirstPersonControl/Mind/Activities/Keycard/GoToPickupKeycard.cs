using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard
{
    internal class GoToPickupKeycard : GoToPickupItemBase<KeycardWithPermissions>
    {
        protected override ItemWithinSight<KeycardWithPermissions> ItemWithinSight => _botPlayer.MindRunner.GetBelief<KeycardWithinSight>(OfPermissions);
        protected override ItemWithinPickupDistance<KeycardWithPermissions> ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<KeycardWithinPickupDistance>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public GoToPickupKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(ItemPickup<ItemPickupBase, KeycardWithPermissions> b) => b.Criteria.Permissions == Permissions;

        public override string ToString()
        {
            return $"{GetType().Name}({Permissions})";
        }
    }
}
