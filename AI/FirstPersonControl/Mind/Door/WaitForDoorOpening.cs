using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class WaitForDoorOpening : IAction
    {
        private DoorObstacle doorObstacleBelief;
        private FpcBotPlayer botPlayer;

        public WaitForDoorOpening(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, bool>(this, b => !b.GetLastUninteractableDoor());
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
