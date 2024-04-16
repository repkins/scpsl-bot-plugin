using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindRunner : FpcMind
    {
        public IActivity RunningActivity { get; private set; }

        private bool isBeliefsUpdated = false;

        public void SubscribeToBeliefUpdates()
        {
            foreach (var belief in Beliefs.Values.SelectMany(bc => bc))
            {
                belief.OnUpdate += () => OnBeliefUpdate(belief);
            }
        }

        public void EvaluateDesiresToActivities()
        {
            IEnumerable<IActivity> enabledActivities = GetEnabledActivitiesTowardsDesires();

            SelectActivityAndRun(enabledActivities);
        }

        public void Tick()
        {
            if (isBeliefsUpdated)
            {
                isBeliefsUpdated = false;

                IEnumerable<IActivity> enabledActivities = GetEnabledActivitiesTowardsDesires();
                SelectActivityAndRun(enabledActivities);
            }

            RunningActivity?.Tick();
        }

        internal void Dump()
        {
            var allDesires = DesiresEnabledByBeliefs.Keys;
            foreach (var desire in allDesires)
            {
                Log.Debug($"Desire: {desire.GetType().Name}");

                var desireEnablingBeliefs = DesiresEnabledByBeliefs[desire];
                DumpBeliefsActivities(desireEnablingBeliefs, "  ");
            }
        }

        private void DumpBeliefsActivities(IEnumerable<IBelief> beliefs, string prefix)
        {
            foreach (var enablingBelief in beliefs)
            {
                Log.Debug($"{prefix}Belief: {enablingBelief}");

                var activities = BeliefsImpactedByActivities[enablingBelief];
                foreach (var (activity, _) in activities)
                {
                    Log.Debug($"{prefix}  Activity: {activity}");

                    var enablingBeliefs = ActivitiesEnabledByBeliefs[activity];
                    DumpBeliefsActivities(enablingBeliefs.Select(t => t.Belief), $"{prefix}    ");
                }
            }
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            isBeliefsUpdated = true;
            Log.Debug($"Belief updated: {updatedBelief}");
        }

        private IEnumerable<IActivity> GetEnabledActivitiesTowardsDesires()
        {
            Log.Debug($"    Getting enabled activities towards desires.");

            var allDesires = DesiresEnabledByBeliefs.Keys;
            var enabledActivities = allDesires
                .Select(d => { 
                    Log.Debug($"    Evaluating desire {d.GetType().Name}"); 
                    return d;
                })
                .Where(d => !d.Condition())
                .Select(d => {
                    Log.Debug($"    Desire {d.GetType().Name} not fulfilled");
                    return d;
                })
                .SelectMany(d => GetClosestActivitiesEnabledBy(DesiresEnabledByBeliefs[d]));

            return enabledActivities;
        }

        private IEnumerable<IActivity> GetClosestActivitiesEnabledBy(IEnumerable<IBelief> beliefs)
        {
            var activitySets = beliefs
                .Select(b => {
                    Log.Debug($"    Getting activities impacting {b.GetType().Name}");
                    return b;
                })
                .Select(b => BeliefsImpactedByActivities[b]
                    .Where(t => !t.Condition(b))
                    .Select(t => t.Activity));

            foreach (var activitiesImpacting in activitySets)
            {
                var enabledActivities = activitiesImpacting
                    .Where(a => ActivitiesEnabledByBeliefs[a].All(t => t.Condition(t.Belief)));

                if (!enabledActivities.Any())
                {
                    enabledActivities = activitiesImpacting
                        .Select(a =>
                        {
                            Log.Debug($"    Activity {a.GetType().Name} needs to be enabled.");
                            return a;
                        })
                        .Select(a => ActivitiesEnabledByBeliefs[a]
                            .Select(t => {
                                Log.Debug($"    Belief {t.Belief.GetType().Name}.");
                                return t;
                            })
                            .Where(t => !t.Condition(t.Belief))
                            .Select(t => t.Belief)
                            .Select(b => {
                                Log.Debug($"    Belief {b.GetType().Name} needs to be satisfied.");
                                return b;
                            }))
                        .SelectMany(GetClosestActivitiesEnabledBy);
                }
                else
                {
                    enabledActivities = enabledActivities
                        .Select(a =>
                        {
                            Log.Debug($"    Activity {a.GetType().Name} conditions fulfilled.");
                            return a;
                        });
                }

                return enabledActivities;
            }

            return Enumerable.Empty<IActivity>();
        }

        private void SelectActivityAndRun(IEnumerable<IActivity> enabledActivities)
        {
            var selectedActivity = enabledActivities//.OrderBy(a => Random.Range(0f, 10f))
                .FirstOrDefault();  // TODO: cost?

            var prevActivity = RunningActivity;

            RunningActivity = selectedActivity ?? null;

            if (RunningActivity != prevActivity)
            {
                RunningActivity?.Reset();
                Log.Debug($"New activity for bot: {RunningActivity}");
            }
        }
    }
}
