using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Scp914;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Desires
{
    internal class StartScp914OnFineSetting : IDesire
    {
        private ItemInIntakeChamber<ItemOfType> _keycardInInventory;
        private Scp914RunningOnSetting _scp914RunningOnSetting;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            //_keycardInInventory = fpcMind.DesireEnabledBy<ItemInIntakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(new (ItemType.KeycardScientist)));
            _scp914RunningOnSetting = fpcMind.DesireEnabledBy<Scp914RunningOnSetting>(this);

        }

        public bool Condition()
        {
            //return _keycardInInventory.PositionRelative.HasValue;
            return _scp914RunningOnSetting.RunningKnobSetting == Scp914KnobSetting.Fine;
        }
    }
}
