using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using PluginAPI.Core.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal record struct ColliderData(int InstanceId, Vector3 Center);

    internal abstract class SightSense<TComponent> : SightSense, ISense where TComponent : Component
    {
        public HashSet<TComponent> ComponentsWithinSight { get; } = new();

        public SightSense(FpcBotPlayer player) : base(player) { }

        protected abstract LayerMask layerMask { get; }

        private readonly Dictionary<ColliderData, TComponent> validCollidersToComponent = new();

        public void ProcessEnter(Collider collider)
        {
            if ((layerMask & (1 << collider.gameObject.layer)) != 0)
            {
                var component = collider.GetComponentInParent<TComponent>();
                if (component != null)
                {                    
                    validCollidersToComponent.Add(new(collider.GetInstanceID(), collider.bounds.center), component);
                }
            }
        }

        public void ProcessExit(Collider collider)
        {
            if ((layerMask & (1 << collider.gameObject.layer)) != 0)
            {
                validCollidersToComponent.Remove(new(collider.GetInstanceID(), collider.bounds.center));
            }
        }

        private readonly List<ColliderData> withinSight = new();

        public IEnumerator<JobHandle> ProcessSensibility()
        {
            ComponentsWithinSight.Clear();

            withinSight.Clear();
            var withinSightHandles = this.GetWithinSight(validCollidersToComponent.Keys, withinSight);
            while (withinSightHandles.MoveNext())
            {
                yield return withinSightHandles.Current;
            }


            foreach (var colliderData in withinSight)
            {
                ComponentsWithinSight.Add(validCollidersToComponent[colliderData]);
            }
        }
    }

    internal abstract class SightSense
    {
        public event Action OnAfterSightSensing;

        public SightSense(FpcBotPlayer botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public abstract void ProcessSightSensedItems();

        public void ProcessSensedItems()
        {
            this.OnAfterSightSensing?.Invoke();

            ProcessSightSensedItems();
        }

        public bool IsPositionObstructed(Vector3 targetPosition) => IsPositionObstructed(targetPosition, out _);

        public bool IsPositionObstructed(Vector3 targetPosition, out RaycastHit outObstructtionHit)
        {
            var cameraPosition = _fpcBotPlayer.CameraPosition;

            var isObstructed = Physics.Linecast(cameraPosition, targetPosition, out var hit, ~excludedCollisionLayerMask);
            if (isObstructed)
            {
                outObstructtionHit = hit;
            }
            else
            {
                outObstructtionHit = default;
            }

            return isObstructed;
        }

        public bool IsPositionWithinFov(Vector3 targetPosition)
        {
            return IsWithinFov(_fpcBotPlayer.CameraPosition, _fpcBotPlayer.CameraForward, targetPosition);
        }

        public float GetDistanceToPosition(Vector3 targetPosition)
        {
            return Vector3.Distance(targetPosition, _fpcBotPlayer.CameraPosition);
        }



        private NativeArray<RaycastCommand> raycastCommandsBuffer = new(1000, Allocator.Persistent);
        private NativeArray<RaycastHit> raycastResultsBuffer = new(1000 * 2, Allocator.Persistent);

        protected IEnumerator<JobHandle> GetWithinSight(ICollection<ColliderData> values, List<ColliderData> withinSights)
        {
            var playerHub = _fpcBotPlayer.BotHub.PlayerHub;

            var cameraPosition = _fpcBotPlayer.CameraPosition;
            var cameraForward = _fpcBotPlayer.CameraForward;

            var colliderDatas = new NativeArray<ColliderData>(values.Count, Allocator.Temp);

            var withinFovJob = new WithinFovJob
            {
                Origin = cameraPosition,
                Direction = cameraForward,
                ColliderDatas = colliderDatas,

                IsWithinFov = new NativeArray<bool>(values.Count, Allocator.Temp)
            };

            var colliderCount = 0;
            foreach (var colliderData in values)
            {
                colliderDatas[colliderCount] = colliderData;
                colliderCount++;
            }

            var withinFovHandle = withinFovJob.ScheduleParallel(colliderCount, 8, default);

            var withinFovColliderDatas = new NativeArray<ColliderData>(colliderCount, Allocator.Temp);
            var filterWithinFovJob = new FilterWithinFovResultsJob
            {
                CameraPosition = cameraPosition,
                ColliderDatas = colliderDatas,
                IsWithinFov = withinFovJob.IsWithinFov,
                ExclusionCollisionMask = excludedCollisionLayerMask,
                ColliderCount = colliderCount,

                RaycastCommands = raycastCommandsBuffer,
                WithinFovColliderDatas = withinFovColliderDatas,
                NumRaycasts = new NativeArray<int>(1, Allocator.Temp),
            };

            var filterWithinFovHandle = filterWithinFovJob.Schedule(withinFovHandle);
            yield return filterWithinFovHandle;

            var numRaycasts = filterWithinFovJob.NumRaycasts[0];

            var raycastCommands = raycastCommandsBuffer.GetSubArray(0, numRaycasts);
            var raycastsJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResultsBuffer, 1);

            var isHits = new NativeArray<bool>(numRaycasts, Allocator.Temp);

            var raycastResultJob = new RaycastResultJob()
            {
                RaycastHit = raycastResultsBuffer,
                ColliderDatas = withinFovColliderDatas,
                IsHit = isHits
            };
            var raycastHitJobHandle = raycastResultJob.Schedule(numRaycasts, 32, raycastsJobHandle);

            yield return raycastHitJobHandle;

            for (var i = 0; i < numRaycasts; i++)
            {
                if (raycastResultJob.IsHit[i])
                {
                    withinSights.Add(withinFovColliderDatas[i]);
                }
            }
        }

        protected static bool IsWithinFov(Transform transform, Transform targetTransform) =>
            IsWithinFov(transform.position, transform.forward, targetTransform.position);

        protected static bool IsWithinFov(Vector3 position, Vector3 forward, Vector3 targetPosition)
        {
            var facingDir = forward;
            var diff = Vector3.Normalize(targetPosition - position);

            if (Vector3.Dot(facingDir, diff) < 0)
            {
                return false;
            }

            if (Vector3.Angle(facingDir, diff) > 90)
            {
                return false;
            }

            return true;
        }

        private readonly FpcBotPlayer _fpcBotPlayer;

        private const int HitsBufferSize = 20;
        private RaycastHit[] _hitsBuffer = new RaycastHit[HitsBufferSize];
        private int _numHits;

        private LayerMask excludedCollisionLayerMask = LayerMask.GetMask("InvisibleCollider", "Hitbox");

        private readonly static Comparer<RaycastHit> distanceComparer = Comparer<RaycastHit>.Create((x, y) => x.distance.CompareTo(y.distance));
    }
}
