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
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, DoorEntry?>(this, c => c!.Value.IsScp914ChamberDoor());
        }

        public float Cost => 5f;

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
        public static bool IsScp914ChamberDoor(this DoorEntry doorEntry)
        {
            return doorEntry.Door is BasicNonInteractableDoor;
        }
    }
}
