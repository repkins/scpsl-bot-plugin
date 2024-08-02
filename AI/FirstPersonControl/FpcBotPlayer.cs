using Interactables;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using PluginAPI.Roles;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Looking;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Movement;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal partial class FpcBotPlayer : IBotPlayer
    {
        public FpcStandardRoleBase FpcRole { get; set; }

        public BotHub BotHub { get; }
        public FpcBotPerception Perception { get; }
        public PerceptionComponent PerceptionComponent { get; private set; }
        public FpcMindRunner MindRunner { get; }

        public FpcBotNavigator Navigator { get; }

        public FpcLook Look { get; }
        public FpcMove Move { get; }

        public Vector3 PlayerPosition { get; private set; }
        public Vector3 PlayerForward { get; private set; }

        public Vector3 CameraPosition { get; private set; }
        public Vector3 CameraForward { get; private set; }

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

        public IEnumerator<JobHandle> Update()
        {
            var playerTransform = FpcRole.transform;
            this.PlayerPosition = playerTransform.position;
            this.PlayerForward = playerTransform.forward;

            var cameraTransform = BotHub.PlayerHub.PlayerCameraReference;
            this.CameraPosition = cameraTransform.position;
            this.CameraForward = cameraTransform.forward;

            var updatePerceptionHandles = Perception.Update();
            while (updatePerceptionHandles.MoveNext())
            {
                yield return updatePerceptionHandles.Current;
            }

            MindRunner.Tick();


            debugStringBuilder.Clear();
            debugStringBuilder.AppendLine("<size=14><align=left>");
            debugStringBuilder.AppendLine("Actions/Beliefs:");
            var numLines = 2;
            //foreach (var belief in MindRunner.Beliefs.Values.SelectMany(bl => bl))

            var action = MindRunner.RunningAction;

            debugStringBuilder.AppendLine($"{action}");
            numLines++;

            while (action != null)
            {
                var beliefsEnabling = MindRunner.ActionsEnabledByBeliefs[action];

                foreach (var belief in beliefsEnabling)
                {
                    if (MindRunner.VisitedActionsEnabledBy.TryGetValue(belief, out var visitedActionEnabledBy)
                        && visitedActionEnabledBy == action)
                    {
                        debugStringBuilder.AppendLine($"  {belief}");
                        numLines++;
                    }
                }

                if (MindRunner.VisitedActionsImpactedBy.TryGetValue(action, out var actionImpacting))
                {
                    debugStringBuilder.AppendLine($"{actionImpacting}");
                    numLines++;

                    action = actionImpacting;
                }
                else
                {
                    action = null;
                }
            }
            debugStringBuilder.Append('\n', Mathf.Max(40 - numLines, 0));

            var debugString = debugStringBuilder.ToString();

            SendTextHintToSpectators(debugString, 10);

            yield break;
        }

        public void OnRoleChanged()
        {
            Log.Info($"Bot got FPC role assigned.");

            PerceptionComponent = BotHub.PlayerHub.GetComponentInChildren<PerceptionComponent>();
            PerceptionComponent.enabled = true;

            MindRunner.EvaluateGoalsToActions();
        }

        public void MoveToPosition(Vector3 goalPosition) => MoveToPosition(goalPosition, out _);
        public void MoveToPosition(Vector3 goalPosition, out Vector3 positionTowardsGoal)
        {
            positionTowardsGoal = Navigator.GetPositionTowards(goalPosition);

            var relativePos = positionTowardsGoal - this.FpcRole.CameraPosition;
            var relativeHorizontalPos = Vector3.ProjectOnPlane(relativePos, Vector3.up);
            var turnPosition = relativeHorizontalPos + this.FpcRole.CameraPosition;

            this.Look.ToPosition(turnPosition);

            var playerDirection = FpcRole.FpcModule.transform.forward;
            var dirTowardsTarget = Vector3.Normalize(relativeHorizontalPos);

            if (Vector3.Dot(playerDirection, dirTowardsTarget) < 0f)
            {
                this.Move.DesiredLocalDirection = FpcRole.FpcModule.transform.InverseTransformDirection(dirTowardsTarget);
            }
            else
            {
                this.Move.DesiredLocalDirection = Vector3.forward;
            }
        }

        public void LookToPosition(Vector3 targetPosition)
        {
            var prevHorizontalRotation = Look.TargetHorizontalRotation;

            Look.ToPosition(targetPosition);

            Move.DesiredLocalDirection = prevHorizontalRotation * Move.DesiredLocalDirection;
        }

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
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, maxInteractDistance, LayerMask.GetMask("Door", "Glass"))
                && hit.collider.GetComponent<InteractableCollider>() is InteractableCollider interactableCollider
                && interactableCollider.Target is DoorVariant interactable)
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

        private readonly StringBuilder debugStringBuilder = new();

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

        private List<Player> players;
        private IEnumerable<Player> spectators;
        public IEnumerable<Player> Spectators
        {
            get
            {
                players ??= Player.GetPlayers();
                spectators ??= players.Where(p => p.RoleBase is OverwatchRole s && s.SyncedSpectatedNetId == this.BotHub.PlayerHub.netId);
                return spectators;
            }
        }

        private int numSpectators;

        private static readonly Dictionary<Player, string> playersHintTexts = new();

        public void SendTextHintToSpectators(string message, float duration)
        {
            var spectatingPlayers = Spectators;
            var actualSpecCount = 0;
            foreach (var spectatingPlayer in spectatingPlayers)
            {
                if (!playersHintTexts.TryGetValue(spectatingPlayer, out var prevHintText))
                {
                    prevHintText = string.Empty;
                    playersHintTexts.Add(spectatingPlayer, prevHintText);
                }
    
                if (prevHintText == message)
                {
                    continue;
                }

                spectatingPlayer.ReceiveHint(message, duration);

                playersHintTexts[spectatingPlayer] = message;

                actualSpecCount++;
            }

            if (actualSpecCount != numSpectators)
            {
                players = null;
                spectators = null;
                numSpectators = actualSpecCount;
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
