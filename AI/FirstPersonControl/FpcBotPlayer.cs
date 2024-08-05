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
using static UnityStandardAssets.Utility.TimedObjectActivator;

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

            DisplayVisitedActionsGraph();

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

        public void DumpMind()
        {
            MindRunner.Dump();
        }

        private readonly StringBuilder debugStringBuilder = new();
        private int numLines;
        private int level;

        private void DisplayVisitedActionsGraph()
        {
            debugStringBuilder.Clear();
            debugStringBuilder.AppendLine("<size=14><align=left>");
            numLines = 0;

            foreach (var (goal, goalEnablingBeliefs) in MindRunner.GoalsEnabledByBeliefs)
            {
                level = 0;
                debugStringBuilder.AppendLine($"Goal: {goal.GetType().Name}");
                numLines++;

                foreach (var goalBelief in goalEnablingBeliefs)
                {
                    if (!MindRunner.VisitedGoalsEnabledBy.ContainsKey(goalBelief))
                    {
                        continue;
                    }

                    ShowVisitedGoalBelief(goalBelief, goal);
                }
            }

            debugStringBuilder.Append('\n', Mathf.Max(40 - numLines, 0));

            var debugString = debugStringBuilder.ToString();

            SendTextHintToSpectators(debugString, 10);
        }

        private void ShowVisitedGoalBelief(IBelief goalBelief, IGoal goal)
        {
            level++;
            //debugStringBuilder.Append(' ', level*2);
            //debugStringBuilder.AppendLine($"{goalBelief}");
            //numLines++;

            foreach (var actionImpacting in MindRunner.BeliefsImpactedByActions[goalBelief])
            {
                if (!MindRunner.VisitedGoalsImpactedBy.TryGetValue(actionImpacting, out var goalImpactedBy)
                    || goalImpactedBy != goal)
                {
                    continue;
                }

                ShowVisitedAction(actionImpacting);
            }
        }

        private void ShowVisitedAction(IAction actionImpacting)
        {
            level++;
            debugStringBuilder.Append(' ', level*4);

            var actionTotalCost = MindRunner.VisitedActionsTotalCosts[actionImpacting];
            if (MindRunner.RelevantActionsImpactingActions.ContainsKey(actionImpacting) || actionImpacting == MindRunner.RunningAction)
            {
                debugStringBuilder.AppendLine($"<color=yellow>{actionImpacting}</color> <b>[{actionTotalCost}]</b>");
            }
            else
            {
                debugStringBuilder.AppendLine($"{actionImpacting} <b>[{actionTotalCost}]</b>");
            }
            numLines++;

            foreach (var beliefEnabling in MindRunner.ActionsEnabledByBeliefs[actionImpacting])
            {
                //if (MindRunner.RelevantBeliefs.Contains(beliefEnabling) && MindRunner.VisitedActionsEnabledBy[beliefEnabling] == actionImpacting)
                //{
                //    debugStringBuilder.Append(' ', level*4+1);
                //    debugStringBuilder.AppendLine($"<color=orange>{beliefEnabling}</color>");
                //    numLines++;
                //}

                ShowVisitedActionsOfBelief(beliefEnabling, actionImpacting);
            }

            level--;
        }

        private void ShowVisitedActionsOfBelief(IBelief belief, IAction actionToEnable)
        {
            foreach (var actionImpacting in MindRunner.BeliefsImpactedByActions[belief])
            {
                if (!MindRunner.VisitedActionsImpactedBy.TryGetValue(actionImpacting, out var actionImpactedBy)
                    || actionImpactedBy != actionToEnable)
                {
                    continue;
                }

                ShowVisitedAction(actionImpacting);
            }
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

        private IEnumerable<ReferenceHub> spectators;
        public IEnumerable<ReferenceHub> Spectators
        {
            get
            {
                spectators ??= ReferenceHub.AllHubs.Where(p => p.roleManager.CurrentRole is OverwatchRole s && s.SyncedSpectatedNetId == this.BotHub.PlayerHub.netId);
                return spectators;
            }
        }

        private static readonly Dictionary<ReferenceHub, string> playersHintTexts = new();

        public void SendTextHintToSpectators(string message, float duration)
        {
            var spectatingPlayers = Spectators;
            foreach (var spectatingHub in spectatingPlayers)
            {
                if (!playersHintTexts.TryGetValue(spectatingHub, out var prevHintText))
                {
                    prevHintText = string.Empty;
                    playersHintTexts.Add(spectatingHub, prevHintText);
                }
    
                if (prevHintText == message)
                {
                    continue;
                }

                Player.Get(spectatingHub).ReceiveHint(message, duration);

                playersHintTexts[spectatingHub] = message;
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
