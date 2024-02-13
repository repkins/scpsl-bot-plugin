using SCPSLBot.AI.FirstPersonControl.Attributes;
using SCPSLBot.AI.FirstPersonControl.Mind.Desires;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class FpcMind
    {
        public Dictionary<IActivity, List<IBelief>> ActivitiesImpactingBeliefs { get; } = new Dictionary<IActivity, List<IBelief>>();
        public Dictionary<IActivity, List<(IBelief Belief, Predicate<IBelief> Condition)>> ActivitiesEnabledByBeliefs { get; } = new ();

        public Dictionary<IDesire, List<IBelief>> DesiresEnabledByBeliefs { get; } = new Dictionary<IDesire, List<IBelief>>();

        public Dictionary<IActivity, Predicate<IBelief>> ActivityEnablingConditions { get; } = new Dictionary<IActivity, Predicate<IBelief>>();

        public Dictionary<Type, IBelief> Beliefs { get; } = new Dictionary<Type, IBelief>();
        public Dictionary<IBelief, List<IActivity>> BeliefsEnablingActivities { get; } = new Dictionary<IBelief, List<IActivity>>();
        public Dictionary<IBelief, List<IActivity>> BeliefsImpactedByActivities { get; } = new Dictionary<IBelief, List<IActivity>>();
        public Dictionary<IBelief, List<IDesire>> BeliefsEnablingDesires { get; } = new Dictionary<IBelief, List<IDesire>>();

        public B ActivityEnabledBy<B>(IActivity activity, Predicate<B> condition) where B : class, IBelief
        {
            var belief = Beliefs[typeof(B)] as B;

            return ActivityEnabledBy(activity, belief, condition);
        }

        public B ActivityImpacts<B>(IActivity activity) where B : class, IBelief
        {
            var belief = Beliefs[typeof(B)];

            ActivityImpacts(activity, belief);

            return belief as B;
        }

        public B DesireEnabledBy<B>(IDesire desire) where B : class, IBelief
        {
            var belief = Beliefs[typeof(B)];

            DesireEnabledBy(desire, belief);

            return belief as B;
        }

        public FpcMind AddActivity(IActivity activity)
        {
            ActivitiesImpactingBeliefs.Add(activity, new());
            ActivitiesEnabledByBeliefs.Add(activity, new());

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
                var condition = attr.Condition;
                ActivityEnabledBy(activity, belief, condition);
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

            var impactedByActivities = new List<IActivity>();
            BeliefsImpactedByActivities.Add(belief, impactedByActivities);

            var enablingDesires = new List<IDesire>();
            BeliefsEnablingDesires.Add(belief, enablingDesires);

            return this;
        }

        public void AddDesire(IDesire desire)
        {
            DesiresEnabledByBeliefs.Add(desire, new List<IBelief>());

            desire.SetEnabledByBeliefs(this);
        }

        public B GetBelief<B>() where B : IBelief
        {
            var belief = Beliefs[typeof(B)];
            return (B)belief;
        }

        public B ActivityEnabledBy<B>(IActivity activity, B belief, Predicate<B> condition) where B : class, IBelief
        {
            var enablesActivities = BeliefsEnablingActivities[belief];
            enablesActivities.Add(activity);

            var enabledBy = ActivitiesEnabledByBeliefs[activity];
            enabledBy.Add((belief, b => condition(b as B)));

            return belief;
        }

        public void ActivityImpacts(IActivity activity, IBelief belief)
        {
            ActivitiesImpactingBeliefs[activity].Add(belief);

            BeliefsImpactedByActivities[belief].Add(activity);
        }

        private void DesireEnabledBy(IDesire desire, IBelief belief)
        {
            var enablesDesires = BeliefsEnablingDesires[belief];
            enablesDesires.Add(desire);

            var enabledBy = DesiresEnabledByBeliefs[desire];
            enabledBy.Add(belief);
        }
    }
}
