using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using System;

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

        private ItemInIntakeChamber<ItemOfType> itemInIntakeChamber;
        private Scp914RunningOnSetting runningOnSetting;
        private ItemInOutakeChamber<C> itemInOutakeChamber;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInIntakeChamber = fpcMind.ActivityEnabledBy<ItemInIntakeChamber<ItemOfType>>(this, b => b.Criteria.Equals(this.InputItemType));
            this.runningOnSetting = fpcMind.ActivityEnabledBy<Scp914RunningOnSetting>(this, b => b.RunningKnobSetting == Setting);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            this.itemInOutakeChamber = fpcMind.ActivityImpacts<ItemInOutakeChamber<C>>(this, b => b.Criteria.Equals(OutputCriteria));
        }

        private float? lastItemsTransformedTime;

        public void Tick()
        {
            if (this.lastItemsTransformedTime != this.runningOnSetting.ItemsTransformedTime)
            {
                var itemRelativePosition = this.itemInIntakeChamber.PositionRelative;
                this.itemInOutakeChamber.Update(itemRelativePosition);
                this.itemInIntakeChamber.Update(null);
            }
        }

        public void Reset()
        {
            this.lastItemsTransformedTime = this.runningOnSetting.ItemsTransformedTime;
        }
    }
}
