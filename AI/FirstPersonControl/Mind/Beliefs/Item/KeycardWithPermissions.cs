using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using InventorySystem;
using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal struct KeycardWithPermissions : IItemBeliefCriteria
    {
        public KeycardPermissions Permissions;
        public KeycardWithPermissions(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }

        public bool EvaluateItem(ItemPickupBase item)
        {
            return InventoryItemLoader.TryGetItem<KeycardItem>(item.Info.ItemId, out var keycard)
                && keycard.Permissions.HasFlag(Permissions);
        }
    }
}
