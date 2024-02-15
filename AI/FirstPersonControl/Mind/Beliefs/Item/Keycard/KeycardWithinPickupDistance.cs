using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinPickupDistance : ItemWithinPickupDistance<KeycardPickup>
    {
        public KeycardPermissions Permissions;
        public KeycardWithinPickupDistance(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }
    }
}
