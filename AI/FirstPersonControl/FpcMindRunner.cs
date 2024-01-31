using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Attributes;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindRunner : FpcMind
    {
        public IActivity RunningActivity { get; private set; }

        public void SubscribeToBeliefUpdates()
        {
            foreach (var belief in Beliefs.Values)
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
            RunningActivity?.Tick();
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            IEnumerable<IActivity> enabledActivities = GetEnabledActivitiesTowardsDesires();

            SelectActivityAndRun(enabledActivities);
        }

        private IEnumerable<IActivity> GetEnabledActivitiesTowardsDesires()
        {
            Log.Debug($"Getting enabled activities towards desires.");

            var allDesires = DesiresEnabledByBeliefs.Keys;
            var enabledActivities = allDesires
                .Select(d => { 
                    Log.Debug($"Evaluating desire {d.GetType().Name}"); 
                    return d;
                })
                .Where(d => !d.Condition())
                .Select(d => {
                    Log.Debug($"Desire {d.GetType().Name} not fulfilled");
                    return d;
                })
                .SelectMany(d => GetClosestActivitiesEnabledBy(DesiresEnabledByBeliefs[d]));

            return enabledActivities;
        }

        private IEnumerable<IActivity> GetClosestActivitiesEnabledBy(IEnumerable<IBelief> beliefs)
        {
            var activities = beliefs
                .Select(b => {
                    Log.Debug($"Getting activities enabled by {b.GetType().Name}");
                    return b;
                })
                .SelectMany(b => BeliefsImpactedByActivities[b]);

            if (activities.Any())
            {
                var enabledActivities = activities
                    .Select(a =>
                    {
                        Log.Debug($"Evaluating activity {a.GetType().Name}");
                        return a;
                    })
                    .Where(a => a.Condition())
                    .Select(a =>
                    {
                        Log.Debug($"Activity {a.GetType().Name} conditions fulfilled.");
                        return a;
                    });

                if (!enabledActivities.Any())
                {
                    var enabledByBeliefs = activities
                        .Select(a =>
                        {
                            Log.Debug($"Activity {a.GetType().Name} needs to be enabled.");
                            return a;
                        })
                        .SelectMany(a => ActivitiesEnabledByBeliefs[a]);

                    enabledActivities = GetClosestActivitiesEnabledBy(enabledByBeliefs);
                }

                return enabledActivities;
            }

            return activities;
        }

        private void SelectActivityAndRun(IEnumerable<IActivity> enabledActivities)
        {
            var selectedActivity = enabledActivities.FirstOrDefault();  // TODO: cost?

            var prevActivity = RunningActivity;

            RunningActivity = selectedActivity ?? null;

            if (RunningActivity != prevActivity)
            {
                RunningActivity?.Reset();
                Log.Debug($"New activity for bot: {RunningActivity?.GetType().Name}");
            }
        }
    }
}
