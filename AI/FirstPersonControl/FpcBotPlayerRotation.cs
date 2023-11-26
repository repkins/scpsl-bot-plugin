using MEC;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal partial class FpcBotPlayer
    {
        public Vector3 DesiredLookAngles { get; set; } = Vector3.zero;

        public void LookToPosition(Vector3 targetPosition)
        {
            var playerTransform = FpcRole.FpcModule.transform;
            var cameraTransform = BotHub.PlayerHub.PlayerCameraReference;

            var relativePos = targetPosition - FpcRole.CameraPosition;

            var hDirectionToTarget = Vector3.ProjectOnPlane(relativePos, Vector3.up).normalized;
            var hForward = playerTransform.forward;

            var hAngleDiff = Vector3.SignedAngle(hDirectionToTarget, hForward, Vector3.down);

            var hReverseRotation = Quaternion.AngleAxis(-hAngleDiff, Vector3.up);

            var vDirectionToTarget = Vector3.Normalize(hReverseRotation * relativePos);
            var vForward = cameraTransform.forward;

            var vAngleDiff = Vector3.SignedAngle(vDirectionToTarget, vForward, cameraTransform.right);

            var targetAngleDiff = new Vector3(vAngleDiff, hAngleDiff);
            var angleDiff = Vector3.MoveTowards(Vector3.zero, targetAngleDiff, Time.deltaTime * 120f);

            DesiredLookAngles = angleDiff;
        }

        public IEnumerator<float> TurnFpcAsync(Vector3 degreesStep, Vector3 targetDegrees)
        {
            var currentMagnitude = 0f;

            var degreesStepMagnitude = degreesStep.magnitude;
            var targetDegreesMagnitude = targetDegrees.magnitude;

            do
            {
                DesiredLookAngles = degreesStep * Time.deltaTime;

                currentMagnitude += degreesStepMagnitude * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
            while (currentMagnitude < targetDegreesMagnitude);

            DesiredLookAngles = Vector3.zero;

            yield break;
        }
    }
}
