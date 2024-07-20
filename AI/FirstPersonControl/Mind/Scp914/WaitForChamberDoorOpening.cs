using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class WaitForChamberDoorOpening : IAction
    {
        private DoorObstacle doorObstacleBelief;
        private FpcBotPlayer botPlayer;

        public WaitForChamberDoorOpening(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, bool>(this, b => b.GetLastScp914ChamberDoor());
        }

        public float Weight = 1f;
        public float Cost => Vector3.Distance(doorObstacleBelief.GetLastScp914ChamberDoor().transform.position, botPlayer.CameraPosition) * Weight;

        public void Tick()
        {
            // TODO: door waiting idle logic
        }

        public void Reset()
        {

        }
    }

    internal static class DoorObstacleExtensions
    {
        public static DoorVariant GetLastScp914ChamberDoor(this DoorObstacle doorObstacle)
        {
            if (doorObstacle.GoalPositions.Count == 0)
            {
                return null;
            }

            var lastDoor = doorObstacle.Doors[doorObstacle.GoalPositions.Last()].Door;
            return lastDoor && lastDoor is BasicNonInteractableDoor ? lastDoor : null;
        }
    }
}
