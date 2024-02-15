using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetO5Keycard : IDesire
    {
        private ItemInInventory _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.DesireEnabledBy<ItemInInventory>(this, OfKeycardO5);
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item;
        }

        private bool OfKeycardO5(ItemInInventory b) => b.ItemType == ItemType.KeycardO5;
    }
}
