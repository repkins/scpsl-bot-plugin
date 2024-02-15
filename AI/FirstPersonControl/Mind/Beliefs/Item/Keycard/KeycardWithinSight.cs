using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinSight : ItemWithinSight<KeycardPickup>
    {
        public KeycardPermissions Permissions;
        public KeycardWithinSight(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }
    }
}
