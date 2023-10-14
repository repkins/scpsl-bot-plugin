using SCPSLBot.AI.FirstPersonControl.Mind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Attributes
{
    internal class ActivityEnabledBy : Attribute
    {
        public ActivityEnabledBy(Type beliefType)
        {
            BeliefType = beliefType;
        }

        public Type BeliefType { get; }
    }

    internal class ActivityEnabledBy<B> : ActivityEnabledBy where B : IBelief
    {
        public ActivityEnabledBy() : base(typeof(B))
        {
        }
    }
}
