using SCPSLBot.AI.FirstPersonControl.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class FpcMind
    {
        public Dictionary<IActivity, List<IBelief>> ActivitiesImpactingBeliefs { get; } = new Dictionary<IActivity, List<IBelief>>();
        public Dictionary<IActivity, Predicate<IBelief>> ActivityEnablingConditions { get; } = new Dictionary<IActivity, Predicate<IBelief>>();
        public Dictionary<Type, IBelief> Beliefs { get; } = new Dictionary<Type, IBelief>();
        public Dictionary<IBelief, List<IActivity>> BeliefsEnablingActivities { get; } = new Dictionary<IBelief, List<IActivity>>();

        public B ActivityEnabledBy<B>(IActivity activity) where B : class, IBelief
        {
            var belief = Beliefs[typeof(B)];

            ActivityEnabledBy(activity, belief);

            return belief as B;
        }

        public B ActivityImpacts<B>(IActivity activity) where B : class, IBelief
        {
            var belief = Beliefs[typeof(B)];

            ActivityImpacts(activity, belief);

            return belief as B;
        }

        public FpcMind AddActivity(IActivity activity)
        {
            ActivitiesImpactingBeliefs.Add(activity, new List<IBelief>());

            var impactsAttributes = activity.GetType().GetCustomAttributes<ActivityImpacts>();
            foreach (var attr in impactsAttributes)
            {
                var beliefType = attr.BeliefType;
                var belief = Beliefs[beliefType];
                ActivityImpacts(activity, belief);
            }

            var enabledByAttributes = activity.GetType().GetCustomAttributes<ActivityEnabledBy>();
            foreach (var attr in enabledByAttributes)
            {
                var beliefType = attr.BeliefType;
                var belief = Beliefs[beliefType];
                ActivityEnabledBy(activity, belief);
            }

            activity.SetImpactsBeliefs(this);
            activity.SetEnabledByBeliefs(this);

            return this;
        }

        public FpcMind AddBelief(IBelief belief)
        {
            Beliefs.Add(belief.GetType(), belief);

            var enablesActivities = new List<IActivity>();
            BeliefsEnablingActivities.Add(belief, enablesActivities);

            return this;
        }

        public B GetBelief<B>() where B : IBelief
        {
            var belief = Beliefs[typeof(B)];
            return (B)belief;
        }

        private void ActivityEnabledBy(IActivity activity, IBelief belief)
        {
            var enablesActivities = BeliefsEnablingActivities[belief];

            enablesActivities.Add(activity);
        }

        private void ActivityImpacts(IActivity activity, IBelief belief)
        {
            if (!ActivitiesImpactingBeliefs.TryGetValue(activity, out var impactingBeliefs))
            {
                impactingBeliefs = new List<IBelief>();
                ActivitiesImpactingBeliefs.Add(activity, impactingBeliefs);
            }

            impactingBeliefs.Add(belief);
        }
    }
}
