using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal struct FilterWithinFovResultsJob : IJob
    {
        [ReadOnly] public Vector3 CameraPosition;
        [ReadOnly] public int ExclusionCollisionMask;

        [ReadOnly] public NativeArray<Vector3> TargetPosition;
        [ReadOnly] public NativeArray<bool> IsWithinFov;
        [ReadOnly] public NativeArray<int> ColliderInstanceIds;

        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;
        [WriteOnly] public NativeArray<int> WithinFovColliderInstancedIds;

        public void Execute()
        {
            var numRaycasts = 0;
            for (var i = 0; i < ColliderInstanceIds.Length; ++i)
            {
                if (IsWithinFov[i])
                {
                    var relPosToItem = TargetPosition[i] - CameraPosition;

                    RaycastCommands[numRaycasts] = new RaycastCommand(CameraPosition, relPosToItem, relPosToItem.magnitude, ~ExclusionCollisionMask);
                    WithinFovColliderInstancedIds[numRaycasts] = ColliderInstanceIds[i];

                    numRaycasts++;
                }
            }
        }
    }
}
