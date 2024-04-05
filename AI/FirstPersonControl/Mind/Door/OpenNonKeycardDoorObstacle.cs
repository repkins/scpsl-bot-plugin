using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class OpenNonKeycardDoorObstacle : IActivity
    {
        private DoorObstacle doorObstacleBelief;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => b.GetLastDoor(KeycardPermissions.None));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<DoorObstacle>(this);
        }

        public void Tick()
        {
            var doorToOpen = doorObstacleBelief.GetLastDoor(KeycardPermissions.None);

            // TODO: door interaction logic
        }

        public void Reset()
        {

        }
    }
}
