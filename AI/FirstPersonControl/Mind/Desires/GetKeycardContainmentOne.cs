using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetKeycardContainmentOne : IDesire
    {
        private ItemInInventory<KeycardWithPermissions> _keycardInInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.DesireEnabledBy<ItemInInventory<KeycardWithPermissions>>(this, OfContainmentOne);
        }

        public bool Condition()
        {
            return _keycardInInventory.Item;
        }

        private bool OfContainmentOne(ItemInInventory<KeycardWithPermissions> b) => b.Criteria.Permissions == KeycardPermissions.ContainmentLevelOne;
    }
}
