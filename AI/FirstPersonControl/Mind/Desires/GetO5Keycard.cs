using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class GetO5Keycard : IDesire
    {
        private ItemOfTypeInInventory _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.DesireEnabledBy<ItemOfTypeInInventory>(this, OfKeycardO5);
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item;
        }

        private bool OfKeycardO5(ItemOfTypeInInventory b) => b.ItemType == ItemType.KeycardO5;
    }
}
