using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IBelief<B, S> : IBelief
    {
        void AddEnablingAction(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter);
        void AddActionImpacting(IAction action, Func<B, S> impactGetter);
    }

    internal interface IBelief
    {
        event Action OnUpdate;
    }

    internal interface IBelief<C> : IBelief
    {
        C Criteria { get; }
    }
}
