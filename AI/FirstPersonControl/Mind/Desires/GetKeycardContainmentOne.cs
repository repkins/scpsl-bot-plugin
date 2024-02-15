using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetKeycardContainmentOne : IDesire
    {
        private ItemInInventory<KeycardItem> _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.DesireEnabledBy<KeycardInInventory>(this, OfContainmentOne);
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item != null;
        }

        private bool OfContainmentOne(KeycardInInventory b) => b.Permissions.HasFlag(KeycardPermissions.ContainmentLevelOne);
    }
}
