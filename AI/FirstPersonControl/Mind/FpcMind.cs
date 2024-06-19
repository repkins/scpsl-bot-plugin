using SCPSLBot.AI.FirstPersonControl.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal class FpcMind
    {
        public Dictionary<IAction, List<IBelief>> ActionsImpactingBeliefs { get; } = new Dictionary<IAction, List<IBelief>>();
        public Dictionary<IAction, List<(IBelief Belief, Predicate<IBelief> Condition)>> ActionsEnabledByBeliefs { get; } = new ();

        public Dictionary<IGoal, List<IBelief>> GoalsEnabledByBeliefs { get; } = new Dictionary<IGoal, List<IBelief>>();

        //public Dictionary<IAction, Predicate<IBelief>> ActionEnablingConditions { get; } = new Dictionary<IAction, Predicate<IBelief>>();

        public Dictionary<Type, List<IBelief>> Beliefs { get; } = new Dictionary<Type, List<IBelief>>();
        public Dictionary<IBelief, List<IAction>> BeliefsEnablingActions { get; } = new Dictionary<IBelief, List<IAction>>();
        public Dictionary<IBelief, List<(IAction Action, Predicate<IBelief> Condition)>> BeliefsImpactedByActions { get; } = new ();
        public Dictionary<IBelief, List<IGoal>> BeliefsEnablingGoals { get; } = new Dictionary<IBelief, List<IGoal>>();

        public B ActionEnabledBy<B, S>(IAction action, Func<B, S> targetGetter, Func<B, S> currentGetter) where B : class, IBelief<B, S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First() as B;

            belief!.AddEnablingAction(action, targetGetter, currentGetter);

            return belief as B;
        }

        public B ActionImpacts<B, S>(IAction action, Func<B, S> impactGetter) where B : class, IBelief<B, S>
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First() as B;

            belief!.AddActionImpacting(action, impactGetter);

            return belief as B;
        }

        public B ActionEnabledBy<B>(IAction action, Predicate<B> condition) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First();

            return ActionEnabledBy(action, belief as B, condition);
        }

        public B ActionEnabledBy<B>(IAction action, Predicate<B> predicate, Predicate<B> condition) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            return ActionEnabledBy(action, belief as B, condition);
        }

        public B ActionEnabledBy<B, C>(IAction action, C criteria, Predicate<B> condition) where B : class, IBelief<C> where C : IEquatable<C>
        {
            var beliefsOfType = Beliefs[typeof(B)].Select(b => (B)b);
            var belief = beliefsOfType.First(b => b.Criteria.Equals(criteria));

            return ActionEnabledBy(action, belief as B, condition);
        }

        public B ActionImpacts<B>(IAction action) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First();

            ActionImpacts(action, belief, b => false);

            return belief as B;
        }

        public B ActionImpacts<B>(IAction action, Predicate<B> predicate) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            ActionImpacts(action, belief, b => false);

            return belief as B;
        }

        public B ActionImpactsWithCondition<B>(IAction action, Predicate<B> condition) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First();

            ActionImpacts(action, belief as B, condition);

            return belief as B;
        }

        public B GoalEnabledBy<B>(IGoal goal) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.First();

            GoalEnabledBy(goal, belief);

            return belief as B;
        }

        public B GoalEnabledBy<B>(IGoal goal, Predicate<B> predicate) where B : class, IBelief
        {
            var beliefsOfType = Beliefs[typeof(B)];
            var belief = beliefsOfType.Find(b => predicate(b as B));

            GoalEnabledBy(goal, belief);

            return belief as B;
        }

        public FpcMind AddAction(IAction action)
        {
            ActionsImpactingBeliefs.Add(action, new());
            ActionsEnabledByBeliefs.Add(action, new());

            var impactsAttributes = action.GetType().GetCustomAttributes<ActionImpacts>();
            foreach (var attr in impactsAttributes)
            {
                var beliefType = attr.BeliefType;
                var belief = Beliefs[beliefType].First();
                var condition = attr.Condition;
                ActionImpacts(action, belief, condition);
            }

            var enabledByAttributes = action.GetType().GetCustomAttributes<ActionEnabledBy>();
            foreach (var attr in enabledByAttributes)
            {
                var beliefType = attr.BeliefType;
                var belief = Beliefs[beliefType].First();
                var condition = attr.Condition;
                ActionEnabledBy(action, belief, condition);
            }

            action.SetImpactsBeliefs(this);
            action.SetEnabledByBeliefs(this);

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

        public B ActionEnabledBy<B>(IAction action, B belief, Predicate<B> condition) where B : class, IBelief
        {
            var enablesActions = BeliefsEnablingActions[belief];
            enablesActions.Add(action);

            var enabledBy = ActionsEnabledByBeliefs[action];
            enabledBy.Add((belief, b => condition(b as B)));

            return belief;
        }

        public void ActionImpacts<B>(IAction action, B belief, Predicate<B> condition) where B : class, IBelief
        {
            ActionsImpactingBeliefs[action].Add(belief);

            BeliefsImpactedByActions[belief].Add((action, b => condition(b as B)));
        }

        private void GoalEnabledBy(IGoal goal, IBelief belief)
        {
            var enablesGoals = BeliefsEnablingGoals[belief];
            enablesGoals.Add(goal);

            var enabledBy = GoalsEnabledByBeliefs[goal];
            enabledBy.Add(belief);
        }
    }
}
