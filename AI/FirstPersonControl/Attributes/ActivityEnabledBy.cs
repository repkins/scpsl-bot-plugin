using SCPSLBot.AI.FirstPersonControl.Mind;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Attributes
{
    internal class ActivityEnabledBy : Attribute
    {
        public ActivityEnabledBy(Type beliefType, Predicate<IBelief> condition)
        {
            BeliefType = beliefType;
            Condition = condition;
        }

        public Type BeliefType { get; }
        public Predicate<IBelief> Condition { get; }
    }

    internal class ActivityEnabledBy<B> : ActivityEnabledBy where B : IBelief
    {
        public ActivityEnabledBy(Predicate<IBelief> condition) : base(typeof(B), condition)
        {
        }
    }
}
