using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetO5Keycard : IDesire
    {
        public KeycardPermissions Permissions { get; }
        public GetO5Keycard(KeycardPermissions permissions)
        {
            Permissions = permissions;
        }

        private KeycardInInventory _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.DesireEnabledBy<KeycardInInventory>(this, OfO5Permissions);
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item;
        }

        private bool OfO5Permissions(KeycardInInventory b) => b.Permissions == Permissions;
    }
}
