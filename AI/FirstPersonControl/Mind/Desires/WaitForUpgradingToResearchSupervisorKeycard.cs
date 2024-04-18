using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class WaitForUpgradingToResearchSupervisorKeycard : IDesire
    {
        private ItemInOutakeChamber<ItemOfType> _itemInOutakeChamber;
        private Scp914RunningOnSetting _scp914RunningOnSetting;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemInOutakeChamber = fpcMind.DesireEnabledBy<ItemInOutakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(new (ItemType.KeycardResearchCoordinator)));
            //_scp914RunningOnSetting = fpcMind.DesireEnabledBy<Scp914RunningOnSetting>(this);

        }

        public bool Condition()
        {
            return _itemInOutakeChamber.PositionRelative.HasValue;
            //return _scp914RunningOnSetting.RunningKnobSetting == Scp914KnobSetting.Fine;
        }
    }
}
