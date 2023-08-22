using System;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcActionTransition
    {
        public IFpcAction From { get; }
        public IFpcAction To { get; }
        private Func<bool> Predicate { get; }

        public Action OnTransition { get; }

        public FpcActionTransition(IFpcAction from, IFpcAction to, Func<bool> predicate, Action onTransition = null)
            : this(to, predicate, onTransition)
        {
            From = from;
        }

        public FpcActionTransition(IFpcAction to, Func<bool> predicate, Action onTransition = null)
        {
            To = to;
            Predicate = predicate;
            OnTransition = onTransition ?? (() => { });
        }

        public bool Evaluate()
        {
            return Predicate.Invoke();
        }
    }
}
