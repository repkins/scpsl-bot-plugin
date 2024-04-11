﻿using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class WaitForDoorOpening : IActivity
    {
        private DoorObstacle doorObstacleBelief;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => b.GetLastUninteractableDoor());
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<DoorObstacle>(this);
        }

        public void Tick()
        {
            // TODO: door waiting idle logic
        }

        public void Reset()
        {

        }
    }
}