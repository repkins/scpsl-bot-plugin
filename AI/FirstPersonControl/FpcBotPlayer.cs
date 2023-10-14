﻿using Hints;
using Interactables;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
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
        public Vector3 DesiredLook { get; set; } = Vector3.zero;

        public FpcBotPlayer(BotHub botHub)
        {
            BotHub = botHub;
            Perception = new FpcBotPerception(this);
            MindRunner = new FpcMindRunner();

            MindRunner.AddBelief(new LastKnownItemLocation<Firearm>());
            MindRunner.AddBelief(new LastKnownItemLocation<Medkit>());
            MindRunner.AddBelief(new LastKnownItemLocation<KeycardItem>());

            //MindRunner.AddBelief(new KeycardWithinSight(KeycardPermissions.ContainmentLevelOne));
            MindRunner.AddBelief(new ItemWithinSight<KeycardItem>());
            MindRunner.AddActivity(new GoToPickupItem<KeycardItem>(this));
            MindRunner.AddActivity(new PickupItem<KeycardItem>(this));

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
                DesiredLook = degreesStep * Time.deltaTime;

                currentMagnitude += degreesStepMagnitude * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
            while (currentMagnitude < targetDegreesMagnitude);

            DesiredLook = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> FindAndApproachFpcAsync()
        {
            yield break;
        }

        #endregion
    }
}
