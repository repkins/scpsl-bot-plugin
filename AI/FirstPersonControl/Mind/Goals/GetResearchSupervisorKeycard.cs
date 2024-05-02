using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Goals
{
    internal class GetResearchSupervisorKeycard : IGoal
    {
        private ItemInInventory<ItemOfType> _itemInInventory;
        private Scp914RunningOnSetting _scp914RunningOnSetting;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemInInventory = fpcMind.GoalEnabledBy<ItemInInventory<ItemOfType>>(this, b => b.Criteria.Equals(new(ItemType.KeycardResearchCoordinator)));
            //_scp914RunningOnSetting = fpcMind.GoalEnabledBy<Scp914RunningOnSetting>(this);

        }

        public bool Condition()
        {
            return _itemInInventory.Item;
            //return _scp914RunningOnSetting.RunningKnobSetting == Scp914KnobSetting.Fine;
        }
    }
}
