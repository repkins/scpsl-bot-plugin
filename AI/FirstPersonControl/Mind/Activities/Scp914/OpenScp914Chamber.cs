using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door.Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class OpenScp914Chamber : IActivity
    {
        public bool Condition() => _keycardInInventory.Item
                                && _closedDoorWithinSight.Door;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.ActivityEnabledBy<KeycardContainmentOneInInventory>(this, b => b.Item);
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<ClosedScp914ChamberWithinSight>(this, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<ClosedScp914ChamberWithinSight>(this);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset() { }

        private KeycardContainmentOneInInventory _keycardInInventory;
        private ClosedScp914ChamberWithinSight _closedDoorWithinSight;
    }
}
