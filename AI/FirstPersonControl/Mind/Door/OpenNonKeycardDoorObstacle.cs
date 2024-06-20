using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class OpenNonKeycardDoorObstacle : IAction
    {
        private DoorObstacle doorObstacleBelief;
        private FpcBotPlayer botPlayer;
        private const float interactDistance = 2f;

        public OpenNonKeycardDoorObstacle(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, bool>(this, b => b.GetLastDoor(KeycardPermissions.None));
        }

        public void Tick()
        {
            var doorToOpen = doorObstacleBelief.GetLastDoor(KeycardPermissions.None, out var goalPos);
            var playerPosition = botPlayer.BotHub.PlayerHub.transform.position;

            if (!doorToOpen)
            {
                Log.Warning($"doorToOpen is null to open");
                return;
            }

            var dist = Vector3.Distance(doorToOpen.transform.position + Vector3.up, playerPosition);
            var isTargetStateOpen = doorToOpen.TargetState;

            if (!isTargetStateOpen && dist <= interactDistance)
            {
                Log.Debug($"{doorToOpen} is within interactable distance");

                if (!botPlayer.OpenDoor(doorToOpen, interactDistance))
                {
                    botPlayer.LookToPosition(doorToOpen.transform.position + Vector3.up * 1f);
                    //Log.Debug($"Looking towards door interactable");
                }
            }

            if (!isTargetStateOpen || dist > interactDistance)
            {
                botPlayer.MoveToPosition(goalPos);
            }
        }

        public void Reset()
        {

        }

        public override string ToString()
        {
            return $"{nameof(OpenNonKeycardDoorObstacle)}";
        }
    }
}
