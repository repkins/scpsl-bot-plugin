using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class DropKeycardScientistInIntakeChamber : IDesire
    {
        private ItemInIntakeChamber<ItemOfType> _keycardInInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.DesireEnabledBy<ItemInIntakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(new (ItemType.KeycardScientist)));
        }

        public bool Condition()
        {
            return _keycardInInventory.PositionRelative.HasValue;
        }
    }
}
