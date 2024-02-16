using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal abstract class ItemOfCriteriaBase<T> : IBelief where T : IBelief
    {
        protected readonly T belief;
        protected ItemOfCriteriaBase(T belief)
        {
            this.belief = belief;
        }

        public event Action OnUpdate
        {
            add
            {
                belief.OnUpdate += value;
            }
            remove
            {
                belief.OnUpdate -= value;
            }
        }
    }
}
