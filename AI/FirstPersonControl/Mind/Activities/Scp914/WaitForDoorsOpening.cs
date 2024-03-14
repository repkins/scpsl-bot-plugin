using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class WaitForDoorsOpening : IActivity
    {
        public WaitForDoorsOpening()
        {
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<OutakeChamberDoor>(this, b => b.Opened);
            fpcMind.ActivityImpacts<IntakeChamberDoor>(this, b => b.Opened);
        }

        public void Tick()
        {
        }

        public void Reset()
        {
        }

    }
}
