using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class OpenScp914Chamber : IActivity
    {
        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.ActivityEnabledBy<KeycardInInventory>(this, OfContainmentLevelOne, b => b.Item);
            _closedDoor = fpcMind.ActivityEnabledBy<Scp914ChamberDoor>(this, OfClosed, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<Scp914ChamberDoor>(this, OfOpened);
        }

        public OpenScp914Chamber(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        private const float interactDistance = 2f;

        public void Tick()
        {
            var playerPosition = _botPlayer.FpcRole.FpcModule.Position;

            var hub = _botPlayer.BotHub.PlayerHub;
            if (hub.inventory.CurInstance != _keycardInInventory.Item)
            {
                hub.inventory.ServerSelectItem(_keycardInInventory.Item.ItemSerial);
            }

            var door = _closedDoor.Door;
            var doorPosition = door.transform.position + Vector3.up;

            if (Vector3.Distance(doorPosition, playerPosition) <= interactDistance)
                {
                    Log.Debug($"{door} is within interactable distance");

                    if (!_botPlayer.OpenDoor(door, interactDistance))
                    {
                        _botPlayer.LookToPosition(doorPosition);
                        //Log.Debug($"Looking towards door interactable");
                    }
                }
                //else
                //{
                _botPlayer.MoveToPosition(doorPosition);
                //}
        }

        public void Reset() { }

        private KeycardInInventory _keycardInInventory;
        private Scp914ChamberDoor _closedDoor;
        private FpcBotPlayer _botPlayer;

        private static bool OfContainmentLevelOne(KeycardInInventory b) => b.Permissions == KeycardPermissions.ContainmentLevelOne;
        private static bool OfClosed(Scp914ChamberDoor b) => b.State == DoorState.Closed;
        private static bool OfOpened(Scp914ChamberDoor b) => b.State == DoorState.Opened;
    }
}
