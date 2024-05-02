using SCPSLBot.AI.FirstPersonControl.Mind;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Attributes
{
    internal class ActionEnabledBy : Attribute
    {
        public ActionEnabledBy(Type beliefType, Predicate<IBelief> condition)
        {
            BeliefType = beliefType;
            Condition = condition;
        }

        public Type BeliefType { get; }
        public Predicate<IBelief> Condition { get; }
    }

    internal class ActionEnabledBy<B> : ActionEnabledBy where B : IBelief
    {
        public ActionEnabledBy(Predicate<IBelief> condition) : base(typeof(B), condition)
        {
        }
    }
}
