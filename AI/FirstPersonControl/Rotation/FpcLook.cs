using MEC;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Looking
{
    internal class FpcLook
    {
        public Vector3 DesiredAngles { get; set; } = Vector3.zero;

        private readonly FpcBotPlayer botPlayer;

        public FpcLook(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void ToPosition(Vector3 targetPosition)
        {
            var playerTransform = botPlayer.FpcRole.FpcModule.transform;
            var cameraTransform = botPlayer.BotHub.PlayerHub.PlayerCameraReference;

            var relativePos = targetPosition - cameraTransform.position;

            var hDirectionToTarget = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
            var hForward = playerTransform.forward;

            var hAngleDiff = Vector3.SignedAngle(hDirectionToTarget, hForward, Vector3.down);

            var hReverseRotation = Quaternion.AngleAxis(-hAngleDiff, Vector3.up);

            var vDirectionToTarget = Vector3.Normalize(hReverseRotation * relativePos);
            var vForward = cameraTransform.forward;

            var vAngleDiff = Vector3.SignedAngle(vDirectionToTarget, vForward, cameraTransform.right);

            var targetAngleDiff = new Vector3(vAngleDiff, hAngleDiff);
            var angleDiff = Vector3.MoveTowards(Vector3.zero, targetAngleDiff, Time.deltaTime * 120f);

            DesiredAngles = angleDiff;
        }

        public IEnumerator<float> ByFpcAsync(Vector3 degreesStep, Vector3 targetDegrees)
        {
            var currentMagnitude = 0f;

            var degreesStepMagnitude = degreesStep.magnitude;
            var targetDegreesMagnitude = targetDegrees.magnitude;

            do
            {
                DesiredAngles = degreesStep * Time.deltaTime;

                currentMagnitude += degreesStepMagnitude * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
            while (currentMagnitude < targetDegreesMagnitude);

            DesiredAngles = Vector3.zero;

            yield break;
        }
    }
}
