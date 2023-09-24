using InventorySystem.Items;
using SCPSLBot.AI.FirstPersonControl.Activities;
using SCPSLBot.AI.FirstPersonControl.Beliefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcMindRunner
    {
        public Dictionary<Type, IBelief> Beliefs { get; } = new Dictionary<Type, IBelief>();
        public Dictionary<IActivity, List<IBelief>> ActivitiesImpactingBeliefs { get; } = new Dictionary<IActivity, List<IBelief>>();
        public Dictionary<IBelief, List<IActivity>> BeliefsEnablingActivities { get; } = new Dictionary<IBelief, List<IActivity>>();

        public IActivity RunningActivity { get; private set; }

        public FpcMindRunner AddBelief(IBelief belief)
        {
            Beliefs.Add(belief.GetType(), belief);

            return this;
        }

        public FpcMindRunner AddActivity(IActivity activity)
        {
            ActivitiesImpactingBeliefs.Add(activity, new List<IBelief>());

            activity.SetImpactsBeliefs(this);
            activity.SetEnabledByBeliefs(this);

            return this;
        }

        public FpcMindRunner ActivityImpacts<B>(IActivity activity) where B : IBelief
        {
            var belief = Beliefs[typeof(B)];

            if (!ActivitiesImpactingBeliefs.TryGetValue(activity, out var impactingBeliefs))
            {
                impactingBeliefs = new List<IBelief>();
                ActivitiesImpactingBeliefs.Add(activity, impactingBeliefs);
            }

            impactingBeliefs.Add(belief);

            return this;
        }

        public FpcMindRunner ActivityEnabledBy<B>(IActivity activity)
        {
            var belief = Beliefs[typeof(B)];

            if (!BeliefsEnablingActivities.TryGetValue(belief, out var enablesActivities))
            {
                enablesActivities = new List<IActivity>();
                BeliefsEnablingActivities.Add(belief, enablesActivities);
            }

            enablesActivities.Add(activity);

            return this;
        }

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

            var activity = enablingActivities.First();  // TODO: cost? conditions?

            RunningActivity = activity;
        }
    }
}
