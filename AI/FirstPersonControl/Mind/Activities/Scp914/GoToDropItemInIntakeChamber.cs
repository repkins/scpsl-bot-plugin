﻿using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class GoToDropItemInIntakeChamber : IActivity
    {
        public readonly ItemType InItemType;
        public GoToDropItemInIntakeChamber(ItemType inItemType)
        {
            this.InItemType = inItemType;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<ItemOfTypeInInventory>(this, b => b.ItemType == InItemType, b => b.Item);
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsInside);
            fpcMind.ActivityEnabledBy<IntakeChamberDoor>(this, b => b.IsOpened, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInIntakeChamber>(this, b => b.ItemType == InItemType);
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