using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal struct RaycastResultJob : IJob
    {
        [ReadOnly] public NativeArray<RaycastHit> RaycastsResult;
        [ReadOnly] public int RaycastsCount;
        [ReadOnly] public NativeArray<ColliderData> ColliderDatas;

        [WriteOnly] public GCHandle WithinSightHandle;

        public void Execute()
        {
            var withinSight = (List<ColliderData>)WithinSightHandle.Target;
            if (withinSight.Count > 0)
            {
                withinSight.Clear();
            }

            for (int i = 0; i < RaycastsCount; i++)
            {
                var hit = RaycastsResult[i];
                if (hit.colliderInstanceID == ColliderDatas[i].InstanceId)
                {
                    withinSight.Add(ColliderDatas[i]);
                }
            }
        }
    }
}
