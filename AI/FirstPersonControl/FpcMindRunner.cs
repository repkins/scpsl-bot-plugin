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

            Log.Debug($"    Getting enabled Actions towards goals.");

            var allGoals = GoalsEnabledByBeliefs.Keys;
            var enabledActions = allGoals
                .Select(d => { 
                    Log.Debug($"    Evaluating goal {d.GetType().Name}"); 
                    return d;
                })
                .Where(d => !d.Condition())
                .Select(d => {
                    Log.Debug($"    Goal {d.GetType().Name} not fulfilled");
                    return d;
                })
                .SelectMany(d => GoalsEnabledByBeliefs[d])
                .Select(b => 
                {
                    RelevantBeliefs.Add(b);

                    return b;
                })
                .SelectMany(b => GetClosestActionsImpacting(b));
            
            Profiler.EndSample();

            return enabledActions;
        }

        private IEnumerable<IAction> GetClosestActionsImpacting(IBelief belief)
        {
            Log.Debug($"    Getting Actions impacting {belief}");

            var actionsImpacting = BeliefsImpactedByActions[belief];

            var enabledActions = actionsImpacting.SelectMany(GetClosestEnablingActions);

            return enabledActions;
        }

        private IEnumerable<IAction> GetClosestEnablingActions(IAction actionToEnable)
        {
            Log.Debug($"    Action {actionToEnable}...");

            var beliefsEnabling = ActionsEnabledByBeliefs[actionToEnable];
            if (beliefsEnabling.All(b => b.EvaluateEnabling(actionToEnable)))
            {
                Log.Debug($"    Action {actionToEnable} conditions fulfilled.");

                foreach (var belief in beliefsEnabling)
                {
                    RelevantBeliefs.Add(belief);
                }

                return Enumerable.Repeat(actionToEnable, 1);
            }
            else
            {
                Log.Debug($"    Action {actionToEnable} needs to be enabled.");

                var enabledActions = beliefsEnabling
                    .Select(b =>
                    {
                        Log.Debug($"    Belief {b}...");

                        RelevantBeliefs.Add(b);

                        return b;
                    })
                    .Where(b => !b.EvaluateEnabling(actionToEnable))
                    .Take(1)
                    .Select(b =>
                    {
                        Log.Debug($"    Belief {b} needs to be satisfied.");
                        return b;
                    })
                    .SelectMany(b => GetClosestActionsImpacting(b, actionToEnable));
                return enabledActions;
            }
        }

        private IEnumerable<IAction> GetClosestActionsImpacting(IBelief belief, IAction actionToEnable)
        {
            Log.Debug($"    Getting Actions impacting {belief} to enable {actionToEnable}");

            var actionsImpacting = BeliefsImpactedByActions[belief]
                .Where(a => belief.EvaluateImpact(a, actionToEnable));

            var enabledActions = actionsImpacting.SelectMany(GetClosestEnablingActions);

            return enabledActions;
        }

        private void SelectActionAndRun(IEnumerable<IAction> enabledActions)
        {
            Profiler.BeginSample($"{nameof(FpcMindRunner)}.{nameof(SelectActionAndRun)}");

            var selectedAction = enabledActions//.OrderBy(a => Random.Range(0f, 10f))
                .FirstOrDefault();  // TODO: cost?

            var prevAction = RunningAction;

            RunningAction = selectedAction ?? null;

            if (RunningAction != prevAction)
            {
                RunningAction?.Reset();
                Log.Debug($"New Action for bot: {RunningAction}");
            }

            Profiler.EndSample();
        }
    }
}
