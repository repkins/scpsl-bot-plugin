using MEC;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl
{
    internal class FpcBotPlayer
    {
        public Vector3 DesiredMoveDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLook { get; set; } = Vector3.zero;

        public FpcBotPlayer()
        {
            _followAction = new FpcBotFollowAction(this);
        }

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

            Log.Info($"degreesStep = {degreesStep}, targetDegrees = {targetDegrees}");

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

        public IEnumerator<float> ApproachFpcAsync(IFpcRole fpcRole)
        {
            ReferenceHub playerHubToApproach = null;

            do
            {
                yield return Timing.WaitForOneFrame;

                if (Physics.Raycast(fpcRole.FpcModule.transform.position, fpcRole.FpcModule.transform.forward, out var hit))
                {
                    if (hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub)
                    {
                        playerHubToApproach = hitHub;
                    }
                }
            }
            while (playerHubToApproach == null);

            _followAction.TargetToFollow = playerHubToApproach;
            _currentAction = _followAction;

            yield return Timing.WaitForSeconds(5f);

            _currentAction = null;

            yield break;
        }

        public void UpdateMovement(IFpcRole fpcRole)
        {
            if (_currentAction != null)
            {
                _currentAction.Update(fpcRole);
            }
        }

        private IFpcBotAction _currentAction;

        private FpcBotFollowAction _followAction;
    }
}
