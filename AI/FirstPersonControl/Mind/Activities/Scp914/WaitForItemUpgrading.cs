using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        private ItemInIntakeChamber itemInIntakeBelief;
        private readonly List<ItemInOutakeChamber> itemInOutakeBeliefs = new();

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            itemInIntakeBelief = fpcMind.ActivityEnabledBy<ItemInIntakeChamber>(this, b => b.ItemType == InItemType, b => b.Inside);
            fpcMind.ActivityEnabledBy<Scp914RunningOnSetting>(this, b => b.Setting == KnobSetting, b => b.RunningAtSetting);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            foreach (var outCriteria in OutItemCriterias)
            {
                itemInOutakeBeliefs.Add(fpcMind.ActivityImpacts<ItemInOutakeChamber>(this, b => b.Criteria.Equals(outCriteria)));
            }
        }

        private float timeRemainingMs;

        public void Reset()
        {
            timeRemainingMs = 10f;
        }

        public void Tick()
        {
            var frameMs = Time.deltaTime;

            if (timeRemainingMs < 0f)
            {
                var dropPosition = itemInIntakeBelief.DropPosition;
                itemInIntakeBelief.Update(isInside: false);
                foreach (var outItemBelief in itemInOutakeBeliefs)
                {
                    outItemBelief.Update(isInside: true, dropPosition);
                }
            }

            timeRemainingMs -= frameMs;
        }

    }
}
