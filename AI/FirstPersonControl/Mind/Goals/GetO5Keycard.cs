using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Goals
{
    internal class GetO5Keycard : IGoal
    {
        private ItemInInventory<ItemOfType> _keycardO5InInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardO5InInventory = fpcMind.GoalEnabledBy<ItemInInventory<ItemOfType>>(this, b => b.Criteria.Equals(ItemType.KeycardO5));
        }

        public bool Condition()
        {
            return _keycardO5InInventory.Item;
        }
    }
}
