﻿using Interactables;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Looking;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            FpcMindFactory.BuildMind(MindRunner, this, Perception);

            MindRunner.SubscribeToBeliefUpdates();
        }

        public void Update()
        {
            Perception.Tick(FpcRole);
            MindRunner.Tick();


            var debugString = "<size=14><align=left>";
            debugString += $"Running action: {MindRunner.RunningAction}\n";
            debugString += "Beliefs: \n";
            var numLines = 2u;
            foreach (var belief in MindRunner.Beliefs.SelectMany(b => b.Value))
            {
                debugString += $"{belief} \n";
                numLines++;
            }
            debugString += "\n\n\n\n\n\n\n\n";
            SendTextHintToSpectators(debugString, 10);
        }

        public void OnRoleChanged()
        {
            Log.Info($"Bot got FPC role assigned.");

            MindRunner.EvaluateGoalsToActions();
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

        public bool Interact(InteractableCollider interactableCollider)
        {
            if (interactableCollider.Target is not IServerInteractable interactable)
            {
                throw new InvalidOperationException("interactableCollider target is not server interactable.");
            }

            var hub = BotHub.PlayerHub;
            var playerCamera = hub.PlayerCameraReference;

            var isHit = interactableCollider
                .GetComponent<Collider>()
                .Raycast(new Ray(playerCamera.position, hub.PlayerCameraReference.forward), out var hit, 2f);

            if (isHit && hit.collider.GetComponent<InteractableCollider>() == interactableCollider)
            {
                interactable.ServerInteract(hub, interactableCollider.ColliderId);

                //Log.Debug($"ServerInteract(...) called on {interactable}");

                return true;
            }

            return false;
        }

        public bool OpenDoor(DoorVariant targetDoor, float maxInteractDistance)
        {
            var hub = BotHub.PlayerHub;
            var playerCamera = hub.PlayerCameraReference;

            //if (firstDoorOnPath.GetComponentsInChildren<Collider>()
            //        .Any(collider => collider.Raycast(new Ray(playerPosition, hub.PlayerCameraReference.forward), out var hit, 2f))
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, maxInteractDistance, LayerMask.GetMask("Door"))
                && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                && hit.collider.GetComponentInParent<DoorVariant>() is DoorVariant interactable)
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

        public void DumpMind()
        {
            MindRunner.Dump();
        }

        string broadcastMessage = string.Empty;

        public void SendBroadcastToSpectators(string message, ushort duration)
        {
            if (broadcastMessage != message)
            {
                broadcastMessage = message;

                var spectatingPlayers = Player.GetPlayers().Where(p => p.RoleBase is OverwatchRole s && s.SyncedSpectatedNetId == this.BotHub.PlayerHub.netId);
                foreach (var spectatingPlayer in spectatingPlayers)
                {
                    spectatingPlayer.SendBroadcast(message, duration, shouldClearPrevious: true);
                }
            }
        }

        string hintText = string.Empty;

        public void SendTextHintToSpectators(string message, float duration)
        {
            if (hintText == message)
            {
                return;
            }

            var spectatingPlayers = Player.GetPlayers().Where(p => p.RoleBase is OverwatchRole s && s.SyncedSpectatedNetId == this.BotHub.PlayerHub.netId);
            foreach (var spectatingPlayer in spectatingPlayers)
            {
                spectatingPlayer.ReceiveHint(message, duration);
            }

            hintText = message;
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
