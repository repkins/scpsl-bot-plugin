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
                foreach (var (Action, _) in Actions)
                {
                    Log.Debug($"{prefix}  Action: {Action}");

                    var enablingBeliefs = ActionsEnabledByBeliefs[Action];
                    DumpBeliefsActions(enablingBeliefs.Select(t => t.Belief), $"{prefix}    ");
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

            //Log.Debug($"    Getting enabled Actions towards desires.");

            var allGoals = GoalsEnabledByBeliefs.Keys;
            var enabledActions = allGoals
                .Select(d => { 
                    //Log.Debug($"    Evaluating desire {d.GetType().Name}"); 
                    return d;
                })
                .Where(d => !d.Condition())
                .Select(d => {
                    //Log.Debug($"    Goal {d.GetType().Name} not fulfilled");
                    return d;
                })
                .SelectMany(d => GoalsEnabledByBeliefs[d])
                .Select(b => 
                {
                    RelevantBeliefs.Add(b);

                    return b;
                })
                .SelectMany(GetClosestActionsImpacting);
            
            Profiler.EndSample();

            return enabledActions;
        }

        private IEnumerable<IAction> GetClosestActionsImpacting(IBelief belief)
        {
            //Log.Debug($"    Getting Actions impacting {belief}");

            var actionsImpacting = BeliefsImpactedByActions[belief]
                .Where(t => !t.Condition(belief))
                .Select(t => t.Action);

            var enabledActions = actionsImpacting.SelectMany(GetClosestEnablingActions);

            return enabledActions;
        }

        private IEnumerable<IAction> GetClosestEnablingActions(IAction actionImpacting)
        {
            //Log.Debug($"    Action {actionImpacting}...");

            var beliefsEnabling = ActionsEnabledByBeliefs[actionImpacting];

            if (beliefsEnabling.All(t => t.Condition(t.Belief)))
            {
                //Log.Debug($"    Action {actionImpacting} conditions fulfilled.");

                foreach (var (belief, _) in beliefsEnabling)
                {
                    RelevantBeliefs.Add(belief);
                }

                return Enumerable.Repeat(actionImpacting, 1);
            }
            else
            {
                //Log.Debug($"    Action {actionImpacting} needs to be enabled.");

                var enabledActions = beliefsEnabling
                    .Select(t =>
                    {
                        //Log.Debug($"    Belief {t.Belief}...");

                        RelevantBeliefs.Add(t.Belief);

                        return t;
                    })
                    .Where(t => !t.Condition(t.Belief))
                    .Take(1)
                    .Select(t => t.Belief)
                    .Select(b =>
                    {
                        //Log.Debug($"    Belief {b} needs to be satisfied.");
                        return b;
                    })
                    .SelectMany(GetClosestActionsImpacting);
                return enabledActions;
            }
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
