using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class Belief<S> : IBelief<S> 
    {
        private readonly Dictionary<IAction, (Func<IBelief, S>, Func<IBelief, S>)> actionsEnabledByGetters = new();
        public void AddEnablingAction<B>(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief
        {
            actionsEnabledByGetters.Add(action, (b => targetGetter(b as B), b => currentGetter(b as B)));
        }

        private readonly Dictionary<IAction, Func<IBelief, S>> actionsImpactingGetters = new();
        public void AddActionImpacting<B>(IAction action, Func<B, S> impactGetter) where B : class, IBelief
        {
            actionsImpactingGetters.Add(action, b => impactGetter(b as B));
        }

        private readonly Dictionary<IGoal, (Func<IBelief, S>, Func<IBelief, S>)> goalsEnabledByGetters = new();
        public void AddEnablingGoal<B>(IGoal goal, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief
        {
            goalsEnabledByGetters.Add(goal, (b => targetGetter(b as B), b => currentGetter(b as B)));
        }

        public bool IsEnabledAction(IAction action)
        {
            var (targetGetter, currentGetter) = actionsEnabledByGetters[action];
            var targetState = targetGetter(this);
            var currentState = currentGetter(this);

            return EqualityComparer<S>.Default.Equals(targetState, currentState);
        }

        public bool CanImpactedByAction(IAction actionImpacting, IAction actionToEnable)
        {
            var (targetGetter, _) = actionsEnabledByGetters[actionToEnable];
            var impactGetter = actionsImpactingGetters[actionImpacting];

            var targetState = targetGetter(this);
            var impactState = impactGetter(this);

            return EqualityComparer<S>.Default.Equals(targetState, impactState);
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
            var impactGetter = actionsImpactingGetters[actionImpacting];

            var targetState = targetGetter(this);
            var impactState = impactGetter(this);

            return EqualityComparer<S>.Default.Equals(targetState, impactState);
        }

        public event Action OnUpdate;
        protected void InvokeOnUpdate()
        {
            OnUpdate?.Invoke();
        }
    }
}
