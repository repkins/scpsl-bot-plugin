using MEC;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TestPlugin.SLBot.FirstPersonControl.Actions;
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
            _findPlayerAction = new FpcBotFindPlayerAction(this);
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

        public IEnumerator<float> FindAndApproachFpcAsync(IFpcRole fpcRole)
        {
            _currentAction = _findPlayerAction;

            yield return Timing.WaitUntilDone(_findPlayerAction.WaitForFoundPlayer());

            ReferenceHub playerHubToApproach = _findPlayerAction.FoundPlayer;

            _followAction.TargetToFollow = playerHubToApproach;
            _currentAction = _followAction;

            yield return Timing.WaitForSeconds(5f);

            _currentAction = null;

            yield break;
        }

        public void Update(IFpcRole fpcRole)
        {
            if (_currentAction != null)
            {
                _currentAction.UpdatePlayer(fpcRole);
            }
        }

        private IFpcBotAction _currentAction;

        private FpcBotFollowAction _followAction;
        private FpcBotFindPlayerAction _findPlayerAction;
    }
}
