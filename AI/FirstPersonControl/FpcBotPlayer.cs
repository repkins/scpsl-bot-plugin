using Hints;
using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotPlayer : IBotPlayer
    {
        public FpcStandardRoleBase FpcRole { get; set; }

        public BotHub BotHub { get; }
        public FpcBotPerception Perception { get; }
        public FpcMindRunner MindRunner { get; }

        public Vector3 DesiredMoveDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLookAngles { get; set; } = Vector3.zero;
        
        public FpcBotPlayer(BotHub botHub)
        {
            BotHub = botHub;
            Perception = new FpcBotPerception(this);
            MindRunner = new FpcMindRunner();

            MindRunner.AddBelief(new LastKnownItemLocation<KeycardItem>());
            MindRunner.AddBelief(new LastKnownItemLocation<Medkit>());
            MindRunner.AddBelief(new LastKnownItemLocation<Firearm>());

            MindRunner.AddBelief(new ItemWithinSight<KeycardPickup>());
            MindRunner.AddBelief(new ItemWithinSightMedkit());
            MindRunner.AddBelief(new ItemWithinPickupDistance<KeycardPickup>());
            MindRunner.AddBelief(new ItemInInventory<KeycardItem>());

            MindRunner.AddBelief(new DoorWithinSight<PryableDoor>());

            MindRunner.AddActivity(new GoToPickupItem<KeycardPickup>(this));
            MindRunner.AddActivity(new PickupItem<KeycardPickup, KeycardItem>(this));

            MindRunner.AddActivity(new Explore(this));

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
        }

        public void LookTowards(Vector3 targetPosition)
        {
            var playerTransform = FpcRole.FpcModule.transform;
            var cameraTransform = BotHub.PlayerHub.PlayerCameraReference;

            var relativePos = targetPosition - FpcRole.CameraPosition;

            var hDirectionToTarget = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
            var hForward = playerTransform.forward;

            var hAngleDiff = Vector3.SignedAngle(hDirectionToTarget, hForward, Vector3.down);

            var hReverseRotation = Quaternion.AngleAxis(-hAngleDiff, Vector3.up);

            var vDirectionToTarget = Vector3.Normalize(hReverseRotation * relativePos);
            var vForward = cameraTransform.forward;

            var vAngleDiff = Vector3.SignedAngle(vDirectionToTarget, vForward, cameraTransform.right);

            DesiredLookAngles = new Vector3(vAngleDiff, hAngleDiff);
        }

        public bool Interact(InteractableCollider interactableCollider)
        {
            if (interactableCollider == null)
            {
                if (!Physics.Raycast(FpcRole.FpcModule.transform.position, DesiredMoveDirection, out var hit))
                {
                    return false;
                }

                interactableCollider = hit.collider.GetComponent<InteractableCollider>();
            }

            if (!interactableCollider)
            {
                return false;
            }

            var interactable = interactableCollider.GetComponentInParent<IServerInteractable>();
            if (interactable == null)
            {
                return false;
            }

            var hub = BotHub.PlayerHub;
            var colliderId = interactableCollider.ColliderId;

            interactable.ServerInteract(hub, colliderId);

            return true;
        }

        #region Debug functions

        public IEnumerator<float> MoveFpcAsync(Vector3 localDirection, int timeAmount)
        {
            var transform = FpcRole.FpcModule.transform;

            DesiredMoveDirection = transform.TransformDirection(localDirection);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredMoveDirection = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> TurnFpcAsync(Vector3 degreesStep, Vector3 targetDegrees)
        {
            var currentMagnitude = 0f;

            var degreesStepMagnitude = degreesStep.magnitude;
            var targetDegreesMagnitude = targetDegrees.magnitude;

            do
            {
                DesiredLookAngles = degreesStep * Time.deltaTime;

                currentMagnitude += degreesStepMagnitude * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
            while (currentMagnitude < targetDegreesMagnitude);

            DesiredLookAngles = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> FindAndApproachFpcAsync()
        {
            yield break;
        }

        #endregion
    }
}
