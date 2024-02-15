using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardInInventory : ItemInInventory<KeycardItem>
    {
        public KeycardPermissions Permissions;
        public KeycardInInventory(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }
    }
}
