using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class EnterScp914Chamber : IActivity
    {
        private Scp914ChamberDoor _closedDoorWithinSight;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _closedDoorWithinSight = fpcMind.ActivityEnabledBy<Scp914ChamberDoor>(this, OfOpened, b => b.Door);

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

        private bool OfOpened(Scp914ChamberDoor obj) => obj.State == DoorState.Opened;
    }
}
