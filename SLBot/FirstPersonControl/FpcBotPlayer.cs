using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using TestPlugin.SLBot.FirstPersonControl.Actions;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl
{
    internal class FpcBotPlayer
    {
        public Vector3 DesiredMoveDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLook { get; set; } = Vector3.zero;

        public event RoleChanged OnChangedRole;

        public FpcBotPlayer()
        {
            SetupActions();
        }

        public void Update(IFpcRole fpcRole)
        {
            var currentActionTransitions = _transitions[_currentAction.GetType()];
            foreach (var transition in currentActionTransitions)
            {
                if (transition.Evaluate())
                {
                    Log.Info($"Transitioning to {transition.To}.");

                    _currentAction = transition.To;
                    _currentAction.OnEnter();
                    transition.OnTransition.Invoke();
                    break;
                }
            }

            _currentAction.UpdatePlayer(fpcRole);
        }

        public void OnRoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            var changedRole = OnChangedRole;
            if (changedRole != null)
            {
                changedRole.Invoke(prevRole, newRole);
            }
        }

        #region Debug functions

        public IEnumerator<float> MoveFpcAsync(IFpcRole fpcRole, Vector3 localDirection, int timeAmount)
        {
            var transform = fpcRole.FpcModule.transform;

            DesiredMoveDirection = transform.TransformDirection(localDirection);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredMoveDirection = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> TurnFpcAsync(IFpcRole _, Vector3 degreesStep, Vector3 targetDegrees)
        {
            var currentMagnitude = 0f;

            var degreesStepMagnitude = degreesStep.magnitude;
            var targetDegreesMagnitude = targetDegrees.magnitude;

            do
            {
                DesiredLook = degreesStep * Time.deltaTime;

                currentMagnitude += degreesStepMagnitude * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
            while (currentMagnitude < targetDegreesMagnitude);

            DesiredLook = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> FindAndApproachFpcAsync(IFpcRole fpcRole)
        {
            yield break;
        }

        #endregion

        private void SetupActions()
        {
            var idleAction = new FpcBotIdleAction(this);
            var findPlayerAction = new FpcBotFindPlayerAction();
            var followAction = new FpcBotFollowAction(this);

            // Setup actions transitions.
            _transitions.Add(idleAction.GetType(), new List<FpcBotActionTransition>()
            {
                new FpcBotActionTransition(idleAction, findPlayerAction,
                    () => idleAction.IsRoleChanged)
            });

            _transitions.Add(findPlayerAction.GetType(), new List<FpcBotActionTransition>()
            {
                new FpcBotActionTransition(findPlayerAction, followAction,
                    () => findPlayerAction.FoundPlayer,
                    () => followAction.TargetToFollow = findPlayerAction.FoundPlayer)
            });

            _transitions.Add(followAction.GetType(), new List<FpcBotActionTransition>());

            // Assign default action.
            _currentAction = idleAction;
        }        

        private IFpcBotAction _currentAction;

        private List<FpcBotActionTransition> _anyTransitions = new List<FpcBotActionTransition>();
        private Dictionary<Type, List<FpcBotActionTransition>> _transitions = new Dictionary<Type, List<FpcBotActionTransition>>();

        public delegate void RoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole);
    }
}
