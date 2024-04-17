using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class WaitForItemUpgrading<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly ItemType InputItemType;
        public readonly C OutputCriteria;
        public readonly Scp914KnobSetting Setting;

        public WaitForItemUpgrading(ItemType inputItemType, C outputCriteria, Scp914KnobSetting setting)
        {
            this.InputItemType = inputItemType;
            this.OutputCriteria = outputCriteria;
            this.Setting = setting;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<ItemInIntakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(this.InputItemType));
            fpcMind.ActivityEnabledBy<Scp914RunningOnSetting>(this, b => b.RunningKnobSetting == Setting);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInOutakeChamber<C>>(this, b => b.Criteria.Equals(OutputCriteria));
        }

        public void Tick()
        {

        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
