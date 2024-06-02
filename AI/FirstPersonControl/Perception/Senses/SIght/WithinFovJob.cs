using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal struct WithinFovJob : IJobFor
    {
        [ReadOnly] public Vector3 Origin;
        [ReadOnly] public Vector3 Direction;
        [ReadOnly] public NativeArray<Vector3> TargetPosition;

        [WriteOnly] public NativeArray<bool> IsWithinFov;

        public void Execute(int index)
        {
            var facingDir = Direction;
            var diff = Vector3.Normalize(TargetPosition[index] - Origin);

            if (Vector3.Dot(facingDir, diff) < 0)
            {
                IsWithinFov[index] = false;
                return;
            }

            if (Vector3.Angle(facingDir, diff) > 90)
            {
                IsWithinFov[index] = false;
                return;
            }

            IsWithinFov[index] = true;
        }
    }
}
