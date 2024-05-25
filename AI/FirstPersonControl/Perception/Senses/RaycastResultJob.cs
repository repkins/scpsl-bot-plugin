using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal struct RaycastResultJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> RaycastHit;
        [ReadOnly] public NativeArray<int> ColliderInstanceIds;

        [WriteOnly] public NativeArray<bool> IsHit;

        public void Execute(int i)
        {
            var hit = RaycastHit[i];
            if (hit.colliderInstanceID == ColliderInstanceIds[i])
            {
                IsHit[i] = true;
            }
            else
            {
                IsHit[i] = false;
            }
        }
    }
}
