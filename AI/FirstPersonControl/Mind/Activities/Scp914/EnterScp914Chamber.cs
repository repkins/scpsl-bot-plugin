using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class EnterScp914Chamber : IActivity
    {
        private Scp914ChamberDoorWithinSight _closedDoorWithinSight;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<Scp914ChamberDoorWithinSight>(this, OfOpened, b => b.Door);

        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<InsideScp914Chamber>(this);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private bool OfOpened(Scp914ChamberDoorWithinSight obj) => obj.State == Beliefs.Door.DoorState.Opened;
    }
}
