using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindRunner : FpcMind
    {
        public IAction RunningAction { get; private set; }
        public float RunningActionCost { get; private set; }

        public readonly HashSet<IBelief> RelevantBeliefs = new();
        private bool isBeliefsUpdated = false;

        public void SubscribeToBeliefUpdates()
        {
            foreach (var belief in Beliefs.Values.SelectMany(bc => bc))
            {
                belief.OnUpdate += () => OnBeliefUpdate(belief);
            }
        }

        public void EvaluateGoalsToActions()
        {
            isBeliefsUpdated = true;
        }

        public void Tick()
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(Tick)}");

            if (isBeliefsUpdated)
            {
                isBeliefsUpdated = false;

                IEnumerable<IAction> enabledActions = GetEnabledActionsTowardsGoals();

                SelectActionAndRun(enabledActions);
            }

            RunningAction?.Tick();

            Profiler.EndSample();
        }

        internal void Dump()
        {
            var allGoals = GoalsEnabledByBeliefs.Keys;
            foreach (var desire in allGoals)
            {
                Log.Debug($"Goal: {desire.GetType().Name}");

                var desireEnablingBeliefs = GoalsEnabledByBeliefs[desire];
                DumpBeliefsActions(desireEnablingBeliefs, "  ");
            }
        }

        private void DumpBeliefsActions(IEnumerable<IBelief> beliefs, string prefix)
        {
            foreach (var enablingBelief in beliefs)
            {
                Log.Debug($"{prefix}Belief: {enablingBelief}");

                var Actions = BeliefsImpactedByActions[enablingBelief];
                foreach (var Action in Actions)
                {
                    Log.Debug($"{prefix}  Action: {Action}");

                    var enablingBeliefs = ActionsEnabledByBeliefs[Action];
                    DumpBeliefsActions(enablingBeliefs, $"{prefix}    ");
                }
            }
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            if (!VisitedActionsEnabledBy.ContainsKey(updatedBelief))
            {
                Log.Debug($"[I] Belief updated: {updatedBelief}");
                return;
            }

            isBeliefsUpdated = true;
            Log.Debug($"[R] Belief updated: {updatedBelief}");
        }

        #region Action Finding

        private readonly Dictionary<IAction, (float Cost, int Level)> visitedActionsTotalCosts = new();
        private readonly Dictionary<IAction, (float Cost, int Level)> remainingActionsToVisit = new();

        public readonly Dictionary<IBelief, IAction> VisitedActionsEnabledBy = new();
        public readonly Dictionary<IBelief, IGoal> VisitedGoalsEnabledBy = new();
        public readonly Dictionary<IAction, IAction> VisitedActionsImpactedBy = new();
        public readonly Dictionary<IAction, IGoal> VisitedGoalsImpactedBy = new();

        public readonly Dictionary<IAction, IAction> RelevantActionsImpactingActions = new();
        public readonly Dictionary<IGoal, IAction> RelevantActionsImpactingGoals = new();

        private IEnumerable<IAction> GetEnabledActionsTowardsGoals()
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(GetEnabledActionsTowardsGoals)}");

            RelevantBeliefs.Clear();

            foreach (var goal in GoalsEnabledByBeliefs.Keys)
            {
                VisitedGoalsEnabledBy.Clear();
                VisitedActionsEnabledBy.Clear();
                VisitedActionsImpactedBy.Clear();
                VisitedGoalsImpactedBy.Clear();

                foreach (var enabledAction in FindEnabledActions(goal))
                {
                    RelevantActionsImpactingActions.Clear();
                    RelevantActionsImpactingGoals.Clear();

                    var actionImpacting = enabledAction;
                    while (VisitedActionsImpactedBy.TryGetValue(actionImpacting, out var actionImpactedBy))
                    {
                        RelevantActionsImpactingActions[actionImpactedBy] = actionImpacting;

                        actionImpacting = actionImpactedBy;
                    }

                    RelevantActionsImpactingGoals[goal] = actionImpacting;

                    yield return enabledAction;
                    break;
                }
            }

            Profiler.EndSample();
        }

        private IEnumerable<IAction> FindEnabledActions(IGoal goal)
        {
            Debug.Log($"  Goal {goal.GetType().Name}...");

            visitedActionsTotalCosts.Clear();
            remainingActionsToVisit.Clear();

            foreach (var b in GoalsEnabledByBeliefs[goal])
            {
                VisitedGoalsEnabledBy[b] = goal;

                if (b.EvaluateEnabling(goal))
                {
                    Debug.Log($"    Belief {b} already satisfies goal.");
                    continue;
                }

                Debug.Log($"    Belief {b} needs to satisfy goal.");

                ProcessActionsImpacting(b, goal);
            }

            while (remainingActionsToVisit.Any())
            {
                var (actionImpacting, actionImpactingTotalCostLevel) = remainingActionsToVisit.Aggregate((a, c) => c.Value.Cost < a.Value.Cost ? c : a);
                remainingActionsToVisit.Remove(actionImpacting);

                visitedActionsTotalCosts[actionImpacting] = actionImpactingTotalCostLevel;

                Debug.Log($"      Visiting action {actionImpacting}.");
                foreach (var enabledAction in GetEnabledActionsEnabling(actionImpacting))
                {
                    yield return enabledAction;
                }
            }
        }

        private void ProcessActionsImpacting(IBelief belief, IGoal goalToEnable)
        {
            foreach (var actionImpacting in BeliefsImpactedByActions[belief])
            {
                VisitedGoalsImpactedBy[actionImpacting] = goalToEnable;

                if (!belief.CanImpactedByAction(actionImpacting, goalToEnable))
                {
                    Debug.Log($"      Action {actionImpacting} cannot impact belief.");
                    continue;
                }

                var actionImpactingCost = actionImpacting.Cost;
                var level = 0;
                remainingActionsToVisit.Add(actionImpacting, (actionImpactingCost, level));

                Debug.Log($"      Action {actionImpacting} can impact belief with cost {actionImpactingCost}.");
            }
        }

        private IEnumerable<IAction> GetEnabledActionsEnabling(IAction actionToEnable)
        {
            var prefix = "      ";

            var beliefsEnabling = ActionsEnabledByBeliefs[actionToEnable];

            var actionEnabled = true;
            foreach (var b in beliefsEnabling)
            {
                VisitedActionsEnabledBy[b] = actionToEnable;

                if (b.IsEnabledAction(actionToEnable))
                {
                    Debug.Log($"{prefix}  Belief {b} already satisfies action.");
                    continue;
                }

                Debug.Log($"{prefix}  Belief {b} needs to satisfy action.");
                actionEnabled = false;

                ProcessActionsImpacting(b, actionToEnable);

                break;
            }

            if (actionEnabled)
            {
                Debug.Log($"{prefix}Action {actionToEnable} conditions fulfilled.");

                yield return actionToEnable;
            }
        }

        private void ProcessActionsImpacting(IBelief belief, IAction actionToEnable)
        {
            var prefix = "        ";

            var (actionToEnableCostToGoal, level) = visitedActionsTotalCosts[actionToEnable];

            foreach (var actionImpacting in BeliefsImpactedByActions[belief])
            {
                if (!belief.CanImpactedByAction(actionImpacting, actionToEnable))
                {
                    Debug.Log($"{prefix}  Action {actionImpacting} cannot impact belief.");
                    continue;
                }

                var actionImpactingCostToGoal = actionToEnableCostToGoal + actionImpacting.Cost;
                level++;
                if (visitedActionsTotalCosts.ContainsKey(actionImpacting) && visitedActionsTotalCosts[actionImpacting].Cost < actionImpactingCostToGoal)
                {
                    Debug.Log($"{prefix}  Action {actionImpacting} can impact belief but cost takes more ({visitedActionsTotalCosts[actionImpacting].Cost} < {actionImpactingCostToGoal}).");
                    continue;
                }

                Debug.Log($"{prefix}  Action {actionImpacting} can impact belief with least total cost {actionImpactingCostToGoal}.");

                remainingActionsToVisit[actionImpacting] = (actionImpactingCostToGoal, level);

                VisitedActionsImpactedBy[actionImpacting] = actionToEnable;
            }
        }

        #endregion

        private void SelectActionAndRun(IEnumerable<IAction> enabledActions)
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(SelectActionAndRun)}");

            var selectedAction = enabledActions.FirstOrDefault();

            var prevAction = RunningAction;

            RunningAction = selectedAction ?? null;
            RunningActionCost = selectedAction?.Cost ?? 0f;

            Log.Debug($"New Action for bot: {RunningAction} (Cost: {RunningActionCost})");

            if (RunningAction != prevAction)
            {
                RunningAction?.Reset();
            }

            Profiler.EndSample();
        }
    }
}
