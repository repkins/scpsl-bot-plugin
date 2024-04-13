using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class OpenNonKeycardDoorObstacle : IActivity
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
            doorObstacleBelief = fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => b.GetLastDoor(KeycardPermissions.None));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<DoorObstacle>(this);
        }

        public void Tick()
        {
            var doorToOpen = doorObstacleBelief.GetLastDoor(KeycardPermissions.None);
            var playerPosition = botPlayer.BotHub.PlayerHub.transform.position;

            if (doorToOpen && !doorToOpen.TargetState && Vector3.Distance(doorToOpen.transform.position + Vector3.up, playerPosition) <= interactDistance)
            {
                Log.Debug($"{doorToOpen} is within interactable distance");

                if (!botPlayer.OpenDoor(doorToOpen, interactDistance))
                {
                    botPlayer.LookToPosition(doorToOpen.transform.position + Vector3.up);
                    //Log.Debug($"Looking towards door interactable");
                }
            }

            if (!doorToOpen.TargetState)
            {
                botPlayer.MoveToPosition(doorObstacleBelief.GetLastGoalPosition(doorToOpen));
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
