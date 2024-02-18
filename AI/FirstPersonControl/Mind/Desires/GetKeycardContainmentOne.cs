using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetKeycardContainmentOne : IDesire
    {
        private ItemInInventory<KeycardItem> _keycardInInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.DesireEnabledBy<KeycardInInventory>(this, OfContainmentOne);
        }

        public bool Condition()
        {
            return _keycardInInventory.Item != null;
        }

        private bool OfContainmentOne(KeycardInInventory b) => b.Permissions == KeycardPermissions.ContainmentLevelOne;
    }
}
