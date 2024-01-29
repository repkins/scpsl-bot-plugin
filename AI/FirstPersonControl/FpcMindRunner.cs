using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Attributes;
using SCPSLBot.AI.FirstPersonControl.Mind;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public void EvaluateAllActivities()
        {
            var activities = ActivitiesImpactingBeliefs.Keys;
            EvaluateActivities(activities);
        }

        public void EvaluateDesiresToActivities()
        {
            var allDesires = DesiresEnabledByBeliefs.Keys;
            var enabledActivities = allDesires.Where(d => !d.Condition())
                .SelectMany(d => SelectEnabledActivities(DesiresEnabledByBeliefs[d]));

            var selectedActivity = enabledActivities.FirstOrDefault();  // TODO: cost?

            var prevActivity = RunningActivity;

            RunningActivity = selectedActivity ?? null;

            if (RunningActivity != prevActivity)
            {
                RunningActivity?.Reset();
            }

            Log.Debug($"New activity for bot: {RunningActivity?.GetType().Name}");

        }

        private IEnumerable<IActivity> SelectEnabledActivities(IEnumerable<IBelief> beliefs)
        {
            var activities = beliefs
                .SelectMany(b => BeliefsEnablingActivities[b]);

            if (activities.Any())
            {
                var enabledActivities = activities.Where(a => a.Condition());
                if (!enabledActivities.Any())
                {
                    enabledActivities = SelectEnabledActivities(enabledActivities.SelectMany(a => ActivitiesEnabledByBeliefs[a]));
                }

                return enabledActivities;
            }

            return activities;
        }

        public void Tick()
        {
            RunningActivity?.Tick();
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            var enablingBeliefs = BeliefsEnablingDesires[updatedBelief];

            var enablingActivities = BeliefsEnablingActivities[updatedBelief];
            EvaluateActivities(enablingActivities);
        }

        private void EvaluateActivities(IEnumerable<IActivity> activities)
        {
            var selectedActivity = activities.FirstOrDefault(a => a.Condition());  // TODO: cost?

            var prevActivity = RunningActivity;

            RunningActivity = selectedActivity ?? (RunningActivity?.Condition() ?? false ? RunningActivity : null);

            if (RunningActivity != prevActivity)
            {
                RunningActivity?.Reset();
            }

            Log.Debug($"New activity for bot: {RunningActivity?.GetType().Name}");
        }
    }
}
