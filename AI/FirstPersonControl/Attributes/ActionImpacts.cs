using SCPSLBot.AI.FirstPersonControl.Mind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ActionImpacts : Attribute
    {
        public ActionImpacts(Type beliefType)
        {
            BeliefType = beliefType;
        }

        public Type BeliefType { get; }
        public Predicate<IBelief> Condition { get; }
    }

    internal class ActionImpacts<B> : ActionImpacts where B : IBelief
    {
        public ActionImpacts() : base(typeof(B))
        {
        }
    }
}
