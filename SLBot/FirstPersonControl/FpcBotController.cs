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
    internal class FpcBotController
    {
        public Vector3 DesiredMoveLocalDirection { get; private set; } = Vector3.zero;
        public Vector3 DesiredLook { get; private set; } = Vector3.zero;

        public IEnumerator<float> MoveFpcAsync(IFpcRole fpcRole, Vector3 direction, int timeAmount)
        {
            var transform = fpcRole.FpcModule.transform;

            DesiredMoveLocalDirection = transform.TransformDirection(direction);

            yield return Timing.WaitForSeconds(timeAmount);

            DesiredMoveLocalDirection = Vector3.zero;

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

        public IEnumerator<float> StartFollowingFpcAsync(IFpcRole fpcRole, ReferenceHub playerHubToFollow)
        {
            Physics.Raycast(fpcRole.FpcModule.transform.position, fpcRole.FpcModule.transform.forward, out var hit);

            //hit.

            DesiredMoveLocalDirection = fpcRole.FpcModule.transform.InverseTransformDirection(playerHubToFollow.transform.position - fpcRole.FpcModule.Position).normalized;
            //DesiredLook = fpcRole.FpcModule.MouseLook.

            yield return Timing.WaitForSeconds(5f);

            DesiredMoveLocalDirection = Vector3.zero;

            yield break;
        }
    }
}
