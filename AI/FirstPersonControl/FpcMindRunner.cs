using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Attributes;
using SCPSLBot.AI.FirstPersonControl.Mind;
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

        public void Tick()
        {
            RunningActivity?.Tick();
        }

        private void OnBeliefUpdate(IBelief updatedBelief)
        {
            var enablingActivities = BeliefsEnablingActivities[updatedBelief];
            var selectedActivity = enablingActivities.FirstOrDefault(a => a.Condition());  // TODO: cost?

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
