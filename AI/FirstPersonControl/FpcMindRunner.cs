using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind;
using System;
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
            IEnumerable<IAction> enabledActions = GetEnabledActionsTowardsGoals();

            SelectActionAndRun(enabledActions);
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
            if (!RelevantBeliefs.Contains(updatedBelief))
            {
                Log.Debug($"[I] Belief updated: {updatedBelief}");
                return;
            }

            isBeliefsUpdated = true;
            Log.Debug($"[R] Belief updated: {updatedBelief}");
        }

        private IEnumerable<IAction> GetEnabledActionsTowardsGoals()
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(GetEnabledActionsTowardsGoals)}");

            RelevantBeliefs.Clear();
            visitingActions.Clear();

            Debug.Log($"Getting enabled Actions towards goals.");

            foreach (var goal in GoalsEnabledByBeliefs.Keys)
            {
                foreach (var enabledAction in GetEnabledActionsTowardsGoal(goal))
                {
                    yield return enabledAction;
                }
            }

            Profiler.EndSample();
        }

        private IEnumerable<IAction> GetEnabledActionsTowardsGoal(IGoal goal)
        {
            Debug.Log($"  Goal {goal.GetType().Name}...");

            foreach (var b in GoalsEnabledByBeliefs[goal])
            {
                RelevantBeliefs.Add(b);

                if (b.EvaluateEnabling(goal))
                {
                    Debug.Log($"    Belief {b} already satisfies goal.");
                    continue;
                }

                Debug.Log($"    Belief {b} needs to satisfy goal.");

                foreach (var enabledAction in GetClosestActionsImpacting(b, goal))
                {
                    yield return enabledAction;
                }
            }
        }

        private IEnumerable<IAction> GetClosestActionsImpacting(IBelief belief, IGoal goalToEnable)
        {
            foreach (var actionImpacting in BeliefsImpactedByActions[belief])
            {
                if (!belief.CanImpactedByAction(actionImpacting, goalToEnable))
                {
                    Debug.Log($"      Action {actionImpacting} cannot impact belief.");
                    continue;
                } 
                else if (visitingActions.Contains(actionImpacting))
                {
                    Debug.Log($"      Action {actionImpacting} can impact but already visited.");
                    continue;
                }

                Debug.Log($"      Action {actionImpacting} can impact belief.");

                foreach (var enabledAction in GetClosestEnabledActionsEnabling(actionImpacting, 3))
                {
                    yield return enabledAction;
                }
            }
        }

        private readonly HashSet<IAction> visitingActions = new();

        private IEnumerable<IAction> GetClosestEnabledActionsEnabling(IAction actionToEnable, int level)
        {
            var prefix = new string(' ', level * 2);

            var beliefsEnabling = ActionsEnabledByBeliefs[actionToEnable];
            
            visitingActions.Add(actionToEnable);

            var actionEnabled = true;
            foreach (var b in beliefsEnabling)
            {
                RelevantBeliefs.Add(b);

                if (b.IsEnabledAction(actionToEnable))
                {
                    Debug.Log($"{prefix}  Belief {b} already satisfies action.");
                    continue;
                }

                Debug.Log($"{prefix}  Belief {b} needs to satisfy action.");

                foreach (var impactingEnabledAction in GetClosestActionsImpacting(b, actionToEnable, level+1))
                {
                    yield return impactingEnabledAction;
                }
                actionEnabled = false;
                break;
            }

            visitingActions.Remove(actionToEnable);

            if (actionEnabled)
            {
                Debug.Log($"{prefix}Action {actionToEnable} conditions fulfilled with cost {actionToEnable.Cost}");

                yield return actionToEnable;
            }
        }

        private readonly Dictionary<(IBelief, IAction), IEnumerable<IAction>> beliefsImpactedByActionsOrdered = new();

        private IEnumerable<IAction> GetClosestActionsImpacting(IBelief belief, IAction actionToEnable, int level)
        {
            var prefix = new string(' ', level * 2);

            if (!beliefsImpactedByActionsOrdered.TryGetValue((belief, actionToEnable), out var actionsImpacting))
            {
                beliefsImpactedByActionsOrdered[(belief, actionToEnable)] = actionsImpacting = BeliefsImpactedByActions[belief]
                    .Where(actionImpacting => 
                    {
                        if (!belief.CanImpactedByAction(actionImpacting, actionToEnable))
                        {
                            Debug.Log($"{prefix}  Action {actionImpacting} cannot impact belief.");
                            return false;
                        }
                        else if (visitingActions.Contains(actionImpacting))
                        {
                            Debug.Log($"{prefix}  Action {actionImpacting} can impact but already visited.");
                            return false;
                        }

                        Debug.Log($"{prefix}  Action {actionImpacting} can impact belief.");
                        return true;
                    })
                    .OrderBy(a => a.Cost);
            }

            foreach (var actionImpacting in actionsImpacting)
            {
                foreach (var enabledAction in GetClosestEnabledActionsEnabling(actionImpacting, level+1))
                {
                    yield return enabledAction;
                    yield break;
                }
            }
        }

        private void SelectActionAndRun(IEnumerable<IAction> enabledActions)
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(SelectActionAndRun)}");

            var selectedAction = enabledActions.OrderBy(a => a.Cost)
                .FirstOrDefault();

            var prevAction = RunningAction;

            RunningAction = selectedAction ?? null;
            RunningActionCost = selectedAction?.Cost ?? 0;

            Log.Debug($"New Action for bot: {RunningAction} (Cost: {RunningActionCost})");

            if (RunningAction != prevAction)
            {
                RunningAction?.Reset();
            }

            Profiler.EndSample();
        }
    }
}
