using Interactables.Interobjects.DoorUtils;
using PluginAPI.Core;
using PluginAPI.Core.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        protected abstract LayerMask LayerMask { get; }

        private readonly Dictionary<ColliderData, TComponent> validCollidersToComponent = new();

        protected virtual TComponent GetComponent(ref Collider collider)
        {
            return collider.GetComponentInParent<TComponent>();
        }

        protected virtual ColliderData GetEnterColliderData(Collider collider)
        { 
            return new(collider.GetInstanceID(), collider.bounds.center);
        }

        protected virtual ColliderData GetExitColliderData(Collider collider)
        {
            return new(collider.GetInstanceID(), collider.bounds.center);
        }

        public void ProcessEnter(Collider collider)
        {
            if ((LayerMask & (1 << collider.gameObject.layer)) != 0)
            {
                var component = GetComponent(ref collider);
                if (component != null)
                {
                    validCollidersToComponent[GetEnterColliderData(collider)] = component;
                }
            }
        }

        public void ProcessExit(Collider collider)
        {
            if ((LayerMask & (1 << collider.gameObject.layer)) != 0)
            {
                var component = GetComponent(ref collider);
                if (component != null)
                {
                    validCollidersToComponent.Remove(GetExitColliderData(collider));
                }
            }
        }

        private readonly List<ColliderData> withinSight = new();

        private GCHandle collidersWithinSightHandle;
        private GCHandle collidersToComponentHandle;
        private GCHandle componentsWithinSightHandle;

        protected SightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            collidersWithinSightHandle = GCHandle.Alloc(withinSight);
            collidersToComponentHandle = GCHandle.Alloc(validCollidersToComponent);
            componentsWithinSightHandle = GCHandle.Alloc(ComponentsWithinSight);
        }

        protected virtual void UpdateColliderData(Dictionary<ColliderData, TComponent> data)
        { }

        public IEnumerator<JobHandle> ProcessSensibility()
        {
            this.UpdateColliderData(validCollidersToComponent);

            yield return this.GetWithinFovHandle(validCollidersToComponent.Keys);
            var raycastsResultHandle = this.GetRaycastsResultHandle(collidersWithinSightHandle);

            var componentsWithinSightJob = new ComponentsWithinSightJob<TComponent>
            {
                CollidersWithinSightHandle = collidersWithinSightHandle,
                CollidersToComponentHandle = collidersToComponentHandle,
                ComponentsWithinSightHandle = componentsWithinSightHandle,
            };

            yield return componentsWithinSightJob.Schedule(raycastsResultHandle);
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

            var isObstructed = Physics.Linecast(cameraPosition, targetPosition, out var hit, collisionLayerMask);
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

        private NativeArray<ColliderData> withinFovColliderDatas;
        private NativeArray<int> numRaycasts;

        protected JobHandle GetWithinFovHandle(ICollection<ColliderData> values)
        {
            var cameraPosition = _fpcBotPlayer.CameraPosition;
            var cameraForward = _fpcBotPlayer.CameraForward;

            var colliderDatas = new NativeArray<ColliderData>(values.Count, Allocator.Temp);

            var colliderCount = 0;
            foreach (var colliderData in values)
            {
                colliderDatas[colliderCount] = colliderData;
                colliderCount++;
            }

            var withinFovJob = new WithinFovJob
            {
                Origin = cameraPosition,
                Direction = cameraForward,
                ColliderDatas = colliderDatas,

                IsWithinFov = new NativeArray<bool>(values.Count, Allocator.Temp)
            };

            var withinFovHandle = withinFovJob.ScheduleParallel(colliderCount, 8, default);

            withinFovColliderDatas = new NativeArray<ColliderData>(colliderCount, Allocator.Temp);
            numRaycasts = new NativeArray<int>(1, Allocator.Temp);
            var filterWithinFovJob = new FilterWithinFovResultsJob
            {
                CameraPosition = cameraPosition,
                ColliderDatas = colliderDatas,
                IsWithinFov = withinFovJob.IsWithinFov,
                CollisionMask = collisionLayerMask,
                ColliderCount = colliderCount,

                RaycastCommands = raycastCommandsBuffer,
                WithinFovColliderDatas = withinFovColliderDatas,
                NumRaycasts = numRaycasts,
            };
            return filterWithinFovJob.Schedule(withinFovHandle);
        }

        private NativeArray<RaycastCommand> raycastCommandsBuffer = new(1000, Allocator.Persistent);
        private NativeArray<RaycastHit> raycastResultsBuffer = new(1000 * 2, Allocator.Persistent);

        protected JobHandle GetRaycastsResultHandle(GCHandle withinSightHandle)
        {
            var numRaycasts = this.numRaycasts[0];

            var raycastCommands = raycastCommandsBuffer.GetSubArray(0, numRaycasts);
            var raycastsJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResultsBuffer, 1);

            var raycastResultJob = new RaycastResultJob()
            {
                RaycastsResult = raycastResultsBuffer,
                RaycastsCount = numRaycasts,
                ColliderDatas = withinFovColliderDatas,
                WithinSightHandle = withinSightHandle,
            };

            return raycastResultJob.Schedule(raycastsJobHandle);
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

        private LayerMask collisionLayerMask = LayerMask.GetMask("Default", "Door", "InteractableNoPlayerCollision", "Glass");
    }
}
