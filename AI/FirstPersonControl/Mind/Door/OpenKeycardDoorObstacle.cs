using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class OpenKeycardDoorObstacle : IAction
    {
        public readonly KeycardPermissions Permissions;
        public OpenKeycardDoorObstacle(KeycardPermissions permissions, FpcBotPlayer botPlayer)
        {
            this.Permissions = permissions;
            this.botPlayer = botPlayer;
        }

        private DoorObstacle doorObstacleBelief;
        private ItemInInventory<KeycardWithPermissions> keycardInInventory;
        private readonly FpcBotPlayer botPlayer;
        private const float interactDistance = 2f;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            keycardInInventory = fpcMind.ActionEnabledBy<ItemInInventory<KeycardWithPermissions>>(this, b => b.Criteria.Equals(new (Permissions)), b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActionImpacts<DoorObstacle, bool>(this, b => b.GetLastDoor(Permissions));
        }

        public void Tick()
        {
            var keycard = keycardInInventory.Item;
            if (!keycard.IsEquipped)
            {
                keycard.Owner.inventory.ServerSelectItem(keycard.ItemSerial);
            }

            var doorToOpen = doorObstacleBelief.GetLastDoor(Permissions, out var goalPos);
            var playerPosition = botPlayer.BotHub.PlayerHub.transform.position;

            if (doorToOpen && !doorToOpen.TargetState)
            {
                var interactablePosition = doorToOpen.transform.position + Vector3.up;

                if (Vector3.Distance(interactablePosition, playerPosition) <= interactDistance)
                {
                    Log.Debug($"{doorToOpen} is within interactable distance");

                    if (!botPlayer.OpenDoor(doorToOpen, interactDistance))
                    {
                        botPlayer.LookToPosition(interactablePosition);
                        //Log.Debug($"Looking towards door interactable");
                    }
                }
            }

            botPlayer.MoveToPosition(goalPos);
        }

        public void Reset()
        {

        }

        public override string ToString()
        {
            return $"{nameof(OpenKeycardDoorObstacle)}({Permissions})";
        }
    }
}
