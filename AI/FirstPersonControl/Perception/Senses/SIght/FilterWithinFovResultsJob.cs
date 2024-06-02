using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal struct FilterWithinFovResultsJob : IJob
    {
        [ReadOnly] public Vector3 CameraPosition;
        [ReadOnly] public int ExclusionCollisionMask;

        [ReadOnly] public NativeArray<Vector3> TargetPosition;
        [ReadOnly] public NativeArray<bool> IsWithinFov;
        [ReadOnly] public NativeArray<int> ColliderInstanceIds;
        [ReadOnly] public int ColliderCount;

        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;
        [WriteOnly] public NativeArray<int> NumRaycasts;
        [WriteOnly] public NativeArray<int> WithinFovColliderInstancedIds;

        public void Execute()
        {
            var numRaycasts = 0;
            for (var colliderIndex = 0; colliderIndex < ColliderCount; colliderIndex++)
            {
                if (IsWithinFov[colliderIndex])
                {
                    var relPosToItem = TargetPosition[colliderIndex] - CameraPosition;

                    RaycastCommands[numRaycasts] = new RaycastCommand(CameraPosition, relPosToItem, relPosToItem.magnitude, ~ExclusionCollisionMask);
                    WithinFovColliderInstancedIds[numRaycasts] = ColliderInstanceIds[colliderIndex];

                    numRaycasts++;
                }
            }

            NumRaycasts[0] = numRaycasts;
        }
    }
}
