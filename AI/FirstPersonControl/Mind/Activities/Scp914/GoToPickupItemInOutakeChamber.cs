using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class GoToPickupItemInOutakeChamber<C> : IActivity where C : IItemBeliefCriteria
    {
        public readonly C Criteria;
        public GoToPickupItemInOutakeChamber(C criteria)
        {
            this.Criteria = criteria;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<ItemInOutakeChamber>(this, b => b.Criteria.Equals(Criteria), b => b.Position.HasValue);
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsInside);
            fpcMind.ActivityEnabledBy<OutakeChamberDoor>(this, b => b.IsOpened, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventory<C>>(this, b => b.Criteria.Equals(Criteria));
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
