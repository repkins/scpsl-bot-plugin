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
            var allGoals = BeliefsEnablingGoals.Keys;
            foreach (var desire in allGoals)
            {
                Log.Debug($"Goal: {desire.GetType().Name}");

                var desireEnablingBeliefs = BeliefsEnablingGoals[desire];
                DumpBeliefsActions(desireEnablingBeliefs, "  ");
            }
        }

        private void DumpBeliefsActions(IEnumerable<IBelief> beliefs, string prefix)
        {
            foreach (var enablingBelief in beliefs)
            {
                Log.Debug($"{prefix}Belief: {enablingBelief}");

                var Actions = ActionsImpactingBeliefs[enablingBelief];
                foreach (var Action in Actions)
                {
                    Log.Debug($"{prefix}  Action: {Action}");

                    var enablingBeliefs = BeliefsEnablingActions[Action];
                    DumpBeliefsActions(enablingBeliefs, $"{prefix}    ");
                }
            }
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            if (!RelevantBeliefs.Contains(updatedBelief))
            {
                Log.Debug($"[I] Belief updated: {updatedBelief}");
                return;
            }

            isBeliefsUpdated = true;
            Log.Debug($"[R] Belief updated: {updatedBelief}");
        }

        #region Action Finding

        public readonly Dictionary<IAction, float> VisitedActionsTotalCosts = new();
        private readonly Dictionary<IAction, float> remainingActionsToExplore = new();

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

            foreach (var goal in BeliefsEnablingGoals.Keys)
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
                        foreach (var visitedBelief in BeliefsEnablingActions[actionImpacting].Where(VisitedActionsEnabledBy.ContainsKey))
                        {
                            RelevantBeliefs.Add(visitedBelief);
                        }

                        actionImpacting = actionImpactedBy;
                    }

                    RelevantActionsImpactingGoals[goal] = actionImpacting;
                    foreach (var visitedBelief in BeliefsEnablingActions[actionImpacting].Where(VisitedActionsEnabledBy.ContainsKey))
                    {
                        RelevantBeliefs.Add(visitedBelief);
                    }

                    yield return enabledAction;
                    break;
                }
            }

            Profiler.EndSample();
        }

        private IEnumerable<IAction> FindEnabledActions(IGoal goal)
        {
            //Debug.Log($"  Goal {goal.GetType().Name}...");

            VisitedActionsTotalCosts.Clear();
            remainingActionsToExplore.Clear();

            foreach (var b in BeliefsEnablingGoals[goal])
            {
                VisitedGoalsEnabledBy[b] = goal;

                if (b.EvaluateEnabling(goal))
                {
                    //Debug.Log($"    Belief {b} already satisfies goal.");
                    continue;
                }

                //Debug.Log($"    Belief {b} needs to satisfy goal.");

                ProcessActionsImpacting(b, goal);
            }

            while (remainingActionsToExplore.Any())
            {
                var actionImpacting = remainingActionsToExplore.Aggregate((a, c) => c.Value < a.Value ? c : a).Key;
                remainingActionsToExplore.Remove(actionImpacting);

                //Debug.Log($"      Exploring action {actionImpacting}.");
                foreach (var enabledAction in GetEnabledActionsEnabling(actionImpacting))
                {
                    yield return enabledAction;
                }
            }
        }

        private void ProcessActionsImpacting(IBelief belief, IGoal goalToEnable)
        {
            foreach (var actionImpacting in ActionsImpactingBeliefs[belief])
            {
                VisitedGoalsImpactedBy[actionImpacting] = goalToEnable;

                if (!belief.CanImpactedByAction(actionImpacting, goalToEnable))
                {
                    //Debug.Log($"      Action {actionImpacting} cannot impact belief.");
                    continue;
                }

                var actionImpactingCost = actionImpacting.Cost;
                remainingActionsToExplore.Add(actionImpacting, actionImpactingCost);
                VisitedActionsTotalCosts[actionImpacting] = actionImpactingCost;

                //Debug.Log($"      Action {actionImpacting} can impact belief with cost {actionImpactingCost}.");
            }
        }

        private IEnumerable<IAction> GetEnabledActionsEnabling(IAction actionToEnable)
        {
            //var prefix = "      ";

            var beliefsEnabling = BeliefsEnablingActions[actionToEnable];

            var actionEnabled = true;
            foreach (var b in beliefsEnabling)
            {
                VisitedActionsEnabledBy[b] = actionToEnable;

                if (b.IsEnabledAction(actionToEnable))
                {
                    //Debug.Log($"{prefix}  Belief {b} already satisfies action.");
                    continue;
                }

                //Debug.Log($"{prefix}  Belief {b} needs to satisfy action.");
                actionEnabled = false;

                ProcessActionsImpacting(b, actionToEnable);

                break;
            }

            if (actionEnabled)
            {
                //Debug.Log($"{prefix}Action {actionToEnable} conditions fulfilled.");

                yield return actionToEnable;
            }
        }

        private void ProcessActionsImpacting(IBelief belief, IAction actionToEnable)
        {
            //var prefix = "        ";

            var actionToEnableCostToGoal = VisitedActionsTotalCosts[actionToEnable];

            foreach (var actionImpacting in ActionsImpactingBeliefs[belief])
            {
                if (!belief.CanImpactedByAction(actionImpacting, actionToEnable))
                {
                    //Debug.Log($"{prefix}  Action {actionImpacting} cannot impact belief.");
                    continue;
                }

                var actionImpactingCostToGoal = actionToEnableCostToGoal + actionImpacting.Cost;
                if (VisitedActionsTotalCosts.ContainsKey(actionImpacting) && VisitedActionsTotalCosts[actionImpacting] < actionImpactingCostToGoal)
                {
                    //Debug.Log($"{prefix}  Action {actionImpacting} can impact belief but cost takes more ({VisitedActionsTotalCosts[actionImpacting]} < {actionImpactingCostToGoal}).");
                    continue;
                }

                //Debug.Log($"{prefix}  Action {actionImpacting} can impact belief with least total cost {actionImpactingCostToGoal}.");

                remainingActionsToExplore[actionImpacting] = actionImpactingCostToGoal;
                VisitedActionsTotalCosts[actionImpacting] = actionImpactingCostToGoal;

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
