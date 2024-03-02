using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class WaitForItemUpgrading : IActivity
    {
        public readonly ItemType InItemType;
        public readonly IEnumerable<IItemBeliefCriteria> OutItemCriterias;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsInside);
            fpcMind.ActivityEnabledBy<ItemInIntakeChamber>(this, b => b.ItemType == InItemType);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<OutakeChamberOpen>(this);

            foreach (var outCriteria in OutItemCriterias)
            {
                var belief = fpcMind.GetBelief<ItemInOutakeChamber>(b => b.Criteria.Equals(outCriteria));
                fpcMind.ActivityImpacts(this, belief);
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
