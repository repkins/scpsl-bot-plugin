using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal struct FilterWithinFovResultsJob : IJob
    {
        [ReadOnly] public Vector3 CameraPosition;
        [ReadOnly] public int ExclusionCollisionMask;

        [ReadOnly] public NativeArray<bool> IsWithinFov;
        [ReadOnly] public NativeArray<ColliderData> ColliderDatas;
        [ReadOnly] public int ColliderCount;

        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;
        [WriteOnly] public NativeArray<int> NumRaycasts;
        [WriteOnly] public NativeArray<ColliderData> WithinFovColliderDatas;

        public void Execute()
        {
            var numRaycasts = 0;
            for (var colliderIndex = 0; colliderIndex < ColliderCount; colliderIndex++)
            {
                if (IsWithinFov[colliderIndex])
                {
                    var relPosToItem = ColliderDatas[colliderIndex].Center - CameraPosition;

                    RaycastCommands[numRaycasts] = new RaycastCommand(CameraPosition, relPosToItem, relPosToItem.magnitude, ~ExclusionCollisionMask);
                    WithinFovColliderDatas[numRaycasts] = ColliderDatas[colliderIndex];

                    numRaycasts++;
                }
            }

            NumRaycasts[0] = numRaycasts;
        }
    }
}
