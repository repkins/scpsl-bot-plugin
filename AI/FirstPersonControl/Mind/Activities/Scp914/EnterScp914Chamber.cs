using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class EnterScp914Chamber : IActivity
    {
        private Scp914ChamberDoor _openedDoorWithinSight;
        private Scp914Chamber _scp914Chamber;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _openedDoorWithinSight = fpcMind.ActivityEnabledBy<Scp914ChamberDoor>(this, OfOpened, b => b.Door);
            _scp914Chamber = fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => !b.IsInside);

        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<Scp914Chamber>(this);
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
