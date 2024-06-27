using UnityEngine;

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
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, bool>(this, b => b.GetLastUninteractableDoor());
        }

        public float Weight = 1f;
        public float Cost => Vector3.Distance(doorObstacleBelief.GetLastUninteractableDoor().transform.position, botPlayer.CameraPosition) * Weight;

        public void Tick()
        {
            // TODO: door waiting idle logic
        }

        public void Reset()
        {

        }
    }
}
