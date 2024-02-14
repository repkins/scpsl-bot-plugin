using Hints;
using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using MapGeneration.Distributors;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Looking;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities.KeycardO5;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door.Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Medkit;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using SCPSLBot.AI.FirstPersonControl.Movement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal partial class FpcBotPlayer : IBotPlayer
    {
        public FpcStandardRoleBase FpcRole { get; set; }

        public BotHub BotHub { get; }
        public FpcBotPerception Perception { get; }
        public FpcMindRunner MindRunner { get; }

        public FpcBotNavigator Navigator { get; }

        public FpcLook Look { get; }
        public FpcMove Move { get; }
        
        public FpcBotPlayer(BotHub botHub)
        {
            BotHub = botHub;
            Perception = new FpcBotPerception(this);
            MindRunner = new FpcMindRunner();

            Navigator = new(this);
            Look = new(this);
            Move = new(this);

            MindRunner.AddBelief(new LastKnownItemLocation<KeycardItem>());
            MindRunner.AddBelief(new LastKnownItemLocation<Medkit>());
            MindRunner.AddBelief(new LastKnownItemLocation<Firearm>());


            MindRunner.AddBelief(new ItemWithinSight(ItemType.KeycardO5));
            MindRunner.AddBelief(new ItemWithinPickupDistance(ItemType.KeycardO5));
            MindRunner.AddBelief(new ItemInInventory(ItemType.KeycardO5));

            MindRunner.AddActivity(new FindItem(ItemType.KeycardO5, this));
            MindRunner.AddActivity(new GoToPickupItem(ItemType.KeycardO5, this));
            MindRunner.AddActivity(new PickupItem(ItemType.KeycardO5, this));


            MindRunner.AddBelief(new KeycardContainmentOneWithinSight());
            MindRunner.AddBelief(new KeycardContainmentOneWithinPickupDistance());
            MindRunner.AddBelief(new KeycardContainmentOneInInventory());

            MindRunner.AddActivity(new FindKeycardContainmentOne(this));
            MindRunner.AddActivity(new GoToPickupKeycardContainmentOne(this));
            MindRunner.AddActivity(new PickupKeycardContainmentOne(this));


            MindRunner.AddBelief(new ItemWithinSight<KeycardPickup>());
            MindRunner.AddBelief(new ItemWithinSightMedkit());
            MindRunner.AddBelief(new ItemWithinPickupDistance<KeycardPickup>());
            MindRunner.AddBelief(new ItemInInventory<KeycardItem>());

            MindRunner.AddActivity(new GoToPickupItem<KeycardPickup>(this));
            MindRunner.AddActivity(new PickupItem<KeycardPickup, KeycardItem>(this));

            MindRunner.AddBelief(new DoorWithinSight<PryableDoor>());
            MindRunner.AddBelief(new ClosedScp914ChamberDoorWithinSight());
            
            //MindRunner.AddActivity(new Explore(this));

            MindRunner.AddDesire(new GetKeycardContainmentOne());
            MindRunner.AddDesire(new GetO5Keycard());

            MindRunner.SubscribeToBeliefUpdates();
        }

        public void Update()
        {
            Perception.Tick(FpcRole);
            MindRunner.Tick();
        }

        public void OnRoleChanged()
        {
            Log.Info($"Bot got FPC role assigned.");

            MindRunner.EvaluateDesiresToActivities();
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            var positionTowardsTarget = Navigator.GetPositionTowards(targetPosition);

            var relativePos = positionTowardsTarget - this.FpcRole.CameraPosition;
            var relativeHorizontalPos = Vector3.ProjectOnPlane(relativePos, Vector3.up);
            var lookPositionTowardsTarget = relativeHorizontalPos + this.FpcRole.CameraPosition;

            this.Look.ToPosition(lookPositionTowardsTarget);

            this.Move.DesiredLocalDirection = Vector3.forward;
        } 

        public void LookToPosition(Vector3 targetPosition) => Look.ToPosition(targetPosition);

        public bool OpenDoor(DoorVariant targetDoor, float maxInteractDistance)
        {
            var hub = BotHub.PlayerHub;
            var playerCamera = hub.PlayerCameraReference;

            //if (firstDoorOnPath.GetComponentsInChildren<Collider>()
            //        .Any(collider => collider.Raycast(new Ray(playerPosition, hub.PlayerCameraReference.forward), out var hit, 2f))
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, maxInteractDistance, LayerMask.GetMask("Door"))
                && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                && hit.collider.GetComponentInParent<IServerInteractable>() is IServerInteractable interactable)
            {
                var colliderId = interactableCollider.ColliderId;

                interactable.ServerInteract(hub, colliderId);
                //Log.Debug($"ServerInteract(...) called on {interactable}");

                return true;
            }

            return false;
        }

        public bool OpenLockerDoor(LockerChamber targetDoor, float maxInteractDistance)
        {
            var hub = BotHub.PlayerHub;
            var playerCamera = hub.PlayerCameraReference;

            var (isHit, hit) = targetDoor.GetComponentsInChildren<InteractableCollider>()
                    .Select(interactableCollider => interactableCollider.GetComponent<Collider>())
                    .Select(collider => (isHit: collider.Raycast(new Ray(playerCamera.position, hub.PlayerCameraReference.forward), out var hit, maxInteractDistance), hit))
                    .FirstOrDefault(t => t.isHit);

            if (isHit
                && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                && hit.collider.GetComponentInParent<IServerInteractable>() is IServerInteractable interactable)

            //if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, maxInteractDistance)
            //    && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
            //    && hit.collider.GetComponentInParent<IServerInteractable>() is IServerInteractable interactable)
            {
                var colliderId = interactableCollider.ColliderId;

                interactable.ServerInteract(hub, colliderId);
                //Log.Debug($"ServerInteract(...) called on {interactable}");

                return true;
            }

            return false;
        }

        #region Debug functions

        public void SendBroadcastToSpectators(string message, ushort duration)
        {
            var spectatingPlayers = Player.GetPlayers().Where(p => p.RoleBase is OverwatchRole s && s.SyncedSpectatedNetId == this.BotHub.PlayerHub.netId);
            foreach (var spectatingPlayer in spectatingPlayers)
            {
                spectatingPlayer.SendBroadcast(message, duration);
            }
        }

        public IEnumerator<float> MoveToFpcAsync(Vector3 localDirection, int timeAmount) => Move.ToFpcAsync(localDirection, timeAmount);
        public IEnumerator<float> LookByFpcAsync(Vector3 degreesStep, Vector3 targetDegrees) => Look.ByFpcAsync(degreesStep, targetDegrees);

        public IEnumerator<float> FindAndApproachFpcAsync()
        {
            yield break;
        }

        #endregion
    }
}
