using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class GoToPickupItemInOutakeChamberOfType : IActivity
    {
        public readonly ItemType ItemType;
        public GoToPickupItemInOutakeChamberOfType(ItemType itemType)
        {
            this.ItemType = itemType;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<ItemInOutakeChamber>(this, b => b.Criteria is ItemOfType, b => b.Position.HasValue);
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsInside);
            fpcMind.ActivityEnabledBy<OutakeChamberDoor>(this, b => b.IsOpened);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemOfTypeInInventory>(this, b => b.ItemType == ItemType);
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
