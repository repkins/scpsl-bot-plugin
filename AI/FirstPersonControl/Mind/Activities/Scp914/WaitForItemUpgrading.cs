using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class WaitForItemUpgrading : IActivity
    {
        public readonly ItemType InItemType;
        public readonly Scp914KnobSetting KnobSetting;
        public readonly IEnumerable<IItemBeliefCriteria> OutItemCriterias;
        public WaitForItemUpgrading(ItemType inItemType, Scp914KnobSetting knobSetting, IEnumerable<IItemBeliefCriteria> outItemCriterias)
        {
            this.InItemType = inItemType;
            this.KnobSetting = knobSetting;
            this.OutItemCriterias = outItemCriterias;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<ItemInIntakeChamber>(this, b => b.ItemType == InItemType);
            fpcMind.ActivityEnabledBy<Scp914RunningOnSetting>(this, b => b.Setting == KnobSetting);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<OutakeChamberDoor>(this, b => b.Opened);

            foreach (var outCriteria in OutItemCriterias)
            {
                fpcMind.ActivityImpacts<ItemInOutakeChamber>(this, b => b.Criteria.Equals(outCriteria));
            }
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

    }
}
