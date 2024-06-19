using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class Belief<B, S> : IBelief<B, S>
    {
        private readonly Dictionary<IAction, (Func<B, S>, Func<B, S>)> actionsEnabledByGetters = new();
        public void AddEnablingAction(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter)
        {
            actionsEnabledByGetters.Add(action, (targetGetter, currentGetter));
        }

        private readonly Dictionary<IAction, Func<B, S>> actionsImpactingGetters = new();
        public void AddActionImpacting(IAction action, Func<B, S> impactGetter)
        {
            actionsImpactingGetters.Add(action, impactGetter);
        }

        public event Action OnUpdate;
        protected void InvokeOnUpdate()
        {
            OnUpdate?.Invoke();
        }
    }
}
