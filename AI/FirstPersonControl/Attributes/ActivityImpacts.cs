using SCPSLBot.AI.FirstPersonControl.Mind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ActivityImpacts : Attribute
    {
        public ActivityImpacts(Type beliefType)
        {
            BeliefType = beliefType;
        }

        public Type BeliefType { get; }
    }

    internal class ActivityImpacts<B> : ActivityImpacts where B : IBelief
    {
        public ActivityImpacts() : base(typeof(B))
        {
        }
    }
}
