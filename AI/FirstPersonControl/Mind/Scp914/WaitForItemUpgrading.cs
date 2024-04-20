using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class WaitForItemUpgrading : IActivity
    {
        public readonly ItemType InputItemType;
        public readonly IItemBeliefCriteria[] OutputCriterias;
        public readonly Scp914KnobSetting Setting;

        public WaitForItemUpgrading(ItemType inputItemType, IEnumerable<IItemBeliefCriteria> outputCriterias, Scp914KnobSetting setting)
        {
            this.InputItemType = inputItemType;
            this.OutputCriterias = outputCriterias.ToArray();
            this.Setting = setting;
        }

        private ItemInIntakeChamber<ItemOfType> itemInIntakeChamber;
        private Scp914RunningOnSetting runningOnSetting;
        private List<ItemInOutakeChamber> itemInOutakeChamberBeliefs = new();

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInIntakeChamber = fpcMind.ActivityEnabledBy<ItemInIntakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(this.InputItemType), b => b.PositionRelative.HasValue);
            this.runningOnSetting = fpcMind.ActivityEnabledBy<Scp914RunningOnSetting>(this, b => b.RunningKnobSetting == Setting);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            foreach (var outputCriteria in this.OutputCriterias)
            {
                var itemInOutakeChamberBelief = fpcMind.ActivityImpacts<ItemInOutakeChamber>(this, b => b.Criteria.Equals(outputCriteria));
                this.itemInOutakeChamberBeliefs.Add(itemInOutakeChamberBelief);
            }
        }

        private float? lastItemsTransformedTime;

        public void Reset()
        {
            this.lastItemsTransformedTime = this.runningOnSetting.ItemsTransformedTime;
        }

        public void Tick()
        {
            if (this.lastItemsTransformedTime != this.runningOnSetting.ItemsTransformedTime)
            {
                if (!this.itemInIntakeChamber.PositionRelative.HasValue)
                {
                    Log.Debug($"{this.itemInIntakeChamber} has no position value.");
                    return;
                }

                var itemRelativePosition = this.itemInIntakeChamber.PositionRelative;
                foreach (var itemInOutakeChamberBelief in this.itemInOutakeChamberBeliefs)
                {
                    itemInOutakeChamberBelief.Update(itemRelativePosition);
                }

                this.itemInIntakeChamber.Update(null);
            }
        }

        public override string ToString()
        {
            return $"{nameof(WaitForItemUpgrading)}({this.InputItemType}, {this.Setting}, ({string.Join<IItemBeliefCriteria>(", ", this.OutputCriterias)}))";
        }
    }
}
