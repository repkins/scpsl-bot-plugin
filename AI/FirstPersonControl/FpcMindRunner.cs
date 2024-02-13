using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind;
using System.Collections.Generic;
using System.Linq;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindRunner : FpcMind
    {
        public IActivity RunningActivity { get; private set; }

        private bool isBeliefsUpdated = false;

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
            if (isBeliefsUpdated)
            {
                isBeliefsUpdated = false;

                IEnumerable<IActivity> enabledActivities = GetEnabledActivitiesTowardsDesires();
                SelectActivityAndRun(enabledActivities);
            }

            RunningActivity?.Tick();
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            isBeliefsUpdated = true;
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
            var beliefActivities = beliefs
                .Select(b => {
                    Log.Debug($"Getting activities enabled by {b.GetType().Name}");
                    return b;
                })
                .Select(b => BeliefsImpactedByActivities[b]);

            foreach (var activities in beliefActivities)
            {
                var enabledActivities = activities
                    .Select(a =>
                    {
                        Log.Debug($"Activity {a.GetType().Name}.");
                        return a;
                    })
                    .Select(a => ActivitiesEnabledByBeliefs[a]
                        .Where(t => !t.Condition(t.Belief))
                        .Select(t => t.Belief)
                        .Select(b => {
                            Log.Debug($"Belief {b.GetType().Name} needs to be satisfied.");
                            return b;
                        }))
                    .SelectMany(GetClosestActivitiesEnabledBy);

                return enabledActivities;
            }

            return Enumerable.Empty<IActivity>();
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
