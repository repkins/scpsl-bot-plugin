using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotPlayer : IBotPlayer
    {
        public FpcStandardRoleBase FpcRole { get; set; }

        public BotHub BotHub { get; }
        public FpcBotPerception Perception { get; }

        public Vector3 DesiredMoveDirection { get; set; } = Vector3.zero;
        public Vector3 DesiredLook { get; set; } = Vector3.zero;

        public FpcBotPlayer(BotHub botHub)
        {
            BotHub = botHub;
            Perception = new FpcBotPerception(this);

            SetupActionTree();
        }

        public void Update()
        {
            Perception.Tick(FpcRole);

            _currentAction.UpdatePlayer();
        }

        public void OnRoleChanged()
        {
            Log.Info($"Bot got FPC role assigned.");
        }

        #region Debug functions

        public IEnumerator<float> MoveFpcAsync(Vector3 localDirection, int timeAmount)
        {
            var transform = FpcRole.FpcModule.transform;

            DesiredMoveDirection = transform.TransformDirection(localDirection);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredMoveDirection = Vector3.zero;

            yield break;
        }

        public IEnumerator<float> TurnFpcAsync(Vector3 degreesStep, Vector3 targetDegrees)
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

        public IEnumerator<float> FindAndApproachFpcAsync()
        {
            yield break;
        }

        #endregion

        private void SetupActionTree()
        {
            // Assign root action (node).
            _currentAction = new FpcPlayAction(this);
        }

        private IFpcAction _currentAction;
    }
}
