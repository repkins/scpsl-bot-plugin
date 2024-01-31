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
            var allDesires = DesiresEnabledByBeliefs.Keys;
            var enabledActivities = allDesires.Where(d => !d.Condition())
                .SelectMany(d => GetClosestActivitiesEnabling(DesiresEnabledByBeliefs[d]));

            SelectActivityAndRun(enabledActivities);
        }

        public void Tick()
        {
            RunningActivity?.Tick();
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            var allDesires = DesiresEnabledByBeliefs.Keys;
            var enabledActivities = allDesires.Where(d => !d.Condition())
                .SelectMany(d => GetClosestActivitiesEnabling(DesiresEnabledByBeliefs[d]));

            SelectActivityAndRun(enabledActivities);
        }

        private IEnumerable<IActivity> GetClosestActivitiesEnabling(IEnumerable<IBelief> beliefs)
        {
            var activities = beliefs
                .SelectMany(b => BeliefsEnablingActivities[b]);

            if (activities.Any())
            {
                var enabledActivities = activities.Where(a => a.Condition());
                if (!enabledActivities.Any())
                {
                    enabledActivities = GetClosestActivitiesEnabling(enabledActivities.SelectMany(a => ActivitiesEnabledByBeliefs[a]));
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
