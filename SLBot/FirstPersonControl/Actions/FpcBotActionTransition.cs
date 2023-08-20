using System;

namespace TestPlugin.SLBot.FirstPersonControl.Actions
{
    internal class FpcBotActionTransition
    {
        public IFpcBotAction From { get; }
        public IFpcBotAction To { get; }
        private Func<bool> Predicate { get; }

        public Action OnTransition { get; }

        public FpcBotActionTransition(IFpcBotAction from, IFpcBotAction to, Func<bool> predicate, Action onTransition = null)
            : this(to, predicate, onTransition)
        {
            From = from;
        }

        public FpcBotActionTransition(IFpcBotAction to, Func<bool> predicate, Action onTransition = null)
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
