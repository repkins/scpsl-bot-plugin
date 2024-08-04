using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class FpcMind
    {
        public Dictionary<IAction, List<IBelief>> ActionsImpactingBeliefs { get; } = new Dictionary<IAction, List<IBelief>>();
        public Dictionary<IAction, List<IBelief>> ActionsEnabledByBeliefs { get; } = new ();

        public Dictionary<IGoal, List<IBelief>> GoalsEnabledByBeliefs { get; } = new Dictionary<IGoal, List<IBelief>>();

        public Dictionary<Type, List<IBelief>> Beliefs { get; } = new Dictionary<Type, List<IBelief>>();
        public Dictionary<IBelief, List<IAction>> BeliefsEnablingActions { get; } = new Dictionary<IBelief, List<IAction>>();
        public Dictionary<IBelief, List<IAction>> BeliefsImpactedByActions { get; } = new ();
        public Dictionary<IBelief, List<IGoal>> BeliefsEnablingGoals { get; } = new Dictionary<IBelief, List<IGoal>>();

        public B ActionEnabledBy<B>(IAction action, Func<B, bool> currentGetter) where B : Belief<bool>
        {
            return ActionEnabledBy(action, targetGetter: b => true, currentGetter);
        }

        public B ActionEnabledBy<B>(IAction action, Predicate<B> predicate, Func<B, bool> currentGetter) where B : Belief<bool>
        {
            return ActionEnabledBy(action, predicate, targetGetter: b => true, currentGetter);
        }

        public B ActionEnabledBy<B, S>(IAction action, Predicate<B> predicate, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            return ActionEnabledBy(action, belief as B, targetGetter, currentGetter);
        }

        public B ActionEnabledBy<B, S>(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single();

            return ActionEnabledBy(action, belief as B, targetGetter, currentGetter);
        }

        public B ActionEnabledBy<B, S>(IAction action, B belief, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            return ActionEnabledBy(action, belief, targetGetter, s => EqualityComparer<S>.Default.Equals(s, currentGetter(belief)));
        }

        public B ActionEnabledBy<B, S>(IAction action, Func<B, S> matchGetter, Predicate<S> matchPredicate) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single();

            return ActionEnabledBy(action, belief as B, matchGetter, matchPredicate);
        }

        public B ActionEnabledBy<B, S>(IAction action, B belief, Func<B, S> matchGetter, Predicate<S> matchPredicate) where B : Belief<S>
        {
            belief.AddEnablingAction(action, matchGetter, matchPredicate);

            BeliefsEnablingActions[belief].Add(action);
            ActionsEnabledByBeliefs[action].Add(belief);

            return belief;
        }

        public B ActionImpacts<B>(IAction action) where B : Belief<bool>
        {
            return ActionImpacts<B, bool>(action, impactGetter: b => true);
        }

        public B ActionImpacts<B>(IAction action, Predicate<B> predicate) where B : Belief<bool>
        {
            return ActionImpacts(action, predicate, impactGetter: b => true);
        }

        public B ActionImpacts<B, S>(IAction action, Predicate<B> predicate, Func<B, S> impactGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            return ActionImpacts(action, belief as B, impactGetter);
        }

        public B ActionImpacts<B, S>(IAction action, Func<B, S> impactGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single();

            return ActionImpacts(action, belief as B, impactGetter);
        }

        public B ActionImpacts<B, S>(IAction action, B belief, Func<B, S> impactGetter) where B : Belief<S>
        {
            return ActionImpacts(action, belief, s => EqualityComparer<S>.Default.Equals(s, impactGetter(belief))) as B;
        }

        public B ActionImpacts<B, S>(IAction action, Predicate<S> matchPredicate) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single() as B;

            return ActionImpacts(action, belief, matchPredicate) as B;
        }

        public Belief<S> ActionImpacts<S>(IAction action, Belief<S> belief, Predicate<S> matchPredicate)
        {
            belief.AddActionImpacting(action, matchPredicate);

            ActionsImpactingBeliefs[action].Add(belief);
            BeliefsImpactedByActions[belief].Add(action);

            return belief;
        }

        public B GoalEnabledBy<B, S>(IGoal goal, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single();

            return GoalEnabledBy(goal, belief as B, targetGetter, currentGetter);
        }

        public B GoalEnabledBy<B, S>(IGoal goal, Predicate<B> predicate, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            return GoalEnabledBy(goal, belief as B, targetGetter, currentGetter);
        }

        public B GoalEnabledBy<B, S>(IGoal goal, B belief, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : Belief<S>
        {
            belief.AddEnablingGoal(goal, targetGetter, currentGetter);

            BeliefsEnablingGoals[belief].Add(goal);
            GoalsEnabledByBeliefs[goal].Add(belief);

            return belief;
        }

        public FpcMind AddAction(IAction action)
        {
            ActionsImpactingBeliefs.Add(action, new());
            ActionsEnabledByBeliefs.Add(action, new());

            action.SetImpactsBeliefs(this);
            action.SetEnabledByBeliefs(this);

            return this;
        }

        public FpcMind AddActions(Func<int, IAction> actionFactory, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                var action = actionFactory.Invoke(i);

                ActionsImpactingBeliefs.Add(action, new());
                ActionsEnabledByBeliefs.Add(action, new());

                action.SetImpactsBeliefs(this);
                action.SetEnabledByBeliefs(this);
            }

            return this;
        }

        public FpcMind AddBelief(IBelief belief)
        {
            if (!Beliefs.TryGetValue(belief.GetType(), out var beliefsOfType))
            {
                beliefsOfType = new();
                Beliefs.Add(belief.GetType(), beliefsOfType);
            }
            beliefsOfType.Add(belief);

            BeliefsEnablingActions.Add(belief, new());
            BeliefsImpactedByActions.Add(belief, new());

            BeliefsEnablingGoals.Add(belief, new());

            return this;
        }

        public void AddGoal(IGoal goal)
        {
            GoalsEnabledByBeliefs.Add(goal, new List<IBelief>());

            goal.SetEnabledByBeliefs(this);
        }

        public B GetBelief<B>() where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Single();

            return belief as B;
        }

        public B GetBelief<B>(Predicate<B> predicate) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            return belief as B;
        }

        public IEnumerable<B> GetBeliefs<B>() where B : class, IBelief
        {
            return Beliefs[typeof(B)].Select(b => b as B);
        }
    }
}
