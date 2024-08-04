using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class Belief<S> : IBelief
    {
        private readonly Dictionary<IAction, (Func<IBelief, S>, Predicate<S>)> actionsEnabledByMatchers = new();
        public void AddEnablingAction<B>(IAction action, Func<B, S> matchGetter, Predicate<S> matchPredicate) where B : class, IBelief
        {
            actionsEnabledByMatchers.Add(action, (b => matchGetter(b as B), s => matchPredicate(s)));
        }

        private readonly Dictionary<IAction, Predicate<S>> actionsImpactingMatchers = new();
        public void AddActionImpacting(IAction action, Predicate<S> matchPredicate)
        {
            actionsImpactingMatchers.Add(action, s => matchPredicate(s));
        }

        private readonly Dictionary<IGoal, (Func<IBelief, S>, Func<IBelief, S>)> goalsEnabledByGetters = new();
        public void AddEnablingGoal<B>(IGoal goal, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief
        {
            goalsEnabledByGetters.Add(goal, (b => targetGetter(b as B), b => currentGetter(b as B)));
        }

        public bool IsEnabledAction(IAction action)
        {
            var (matchGetter, matchPredicate) = actionsEnabledByMatchers[action];

            return matchPredicate(matchGetter(this));
        }

        public bool CanImpactedByAction(IAction actionImpacting, IAction actionToEnable)
        {
            var (enablingMatchGetter, _) = actionsEnabledByMatchers[actionToEnable];
            var impactMatchPredicate = actionsImpactingMatchers[actionImpacting];

            return impactMatchPredicate(enablingMatchGetter(this));
        }

        public bool EvaluateEnabling(IGoal goal)
        {
            var (targetGetter, currentGetter) = goalsEnabledByGetters[goal];
            var targetState = targetGetter(this);
            var currentState = currentGetter(this);

            return EqualityComparer<S>.Default.Equals(targetState, currentState);
        }

        public bool CanImpactedByAction(IAction actionImpacting, IGoal goalToEnable)
        {
            var (targetGetter, _) = goalsEnabledByGetters[goalToEnable];
            var impactMatchPredicate = actionsImpactingMatchers[actionImpacting];

            return impactMatchPredicate(targetGetter(this));
        }

        public event Action OnUpdate;
        protected void InvokeOnUpdate()
        {
            OnUpdate?.Invoke();
        }
    }
}
