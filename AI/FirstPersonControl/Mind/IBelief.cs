using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IBelief<S> : IBelief
    {
        void AddEnablingAction<B>(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief;
        void AddActionImpacting<B>(IAction action, Func<B, S> impactGetter) where B : class, IBelief;
        void AddEnablingGoal<B>(IGoal goal, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief;
    }

    internal interface IBelief
    {
        event Action OnUpdate;

        bool IsEnabledAction(IAction action);
        bool EvaluateEnabling(IGoal goal);
        bool CanImpactedByAction(IAction actionImpacting, IAction actionToEnable);
        bool CanImpactedByAction(IAction actionImpacting, IGoal goalToEnable);
    }
}
