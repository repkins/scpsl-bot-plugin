using PluginAPI.Core;
using PluginAPI.Core.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal abstract class SightSense : ISense
    {
        public event Action OnAfterSightSensing;

        public SightSense(FpcBotPlayer botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public abstract void Reset();
        public abstract void ProcessSensibility(IEnumerable<Collider> collider);
        public abstract void ProcessSightSensedItems();

        public virtual void ProcessSensedItems()
        {
            this.OnAfterSightSensing?.Invoke();

            this.ProcessSightSensedItems();
        }

        public bool IsPositionObstructed(Vector3 targetPosition) => this.IsPositionObstructed(targetPosition, out _);

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

        private const int RaycastMaxHits = 2;

        private NativeArray<RaycastCommand> raycastCommandsBuffer = new(1000, Allocator.Persistent);
        private NativeArray<RaycastHit> raycastResultsBuffer = new(1000 * RaycastMaxHits, Allocator.Persistent);

        private int numRaycasts;

        private readonly List<Collider> sightColliders = new();

        protected IEnumerable<Collider> GetWithinSight(ICollection<Collider> values)
        {
            var playerHub = _fpcBotPlayer.BotHub.PlayerHub;

            var cameraPosition = _fpcBotPlayer.CameraPosition;
            var cameraForward = _fpcBotPlayer.CameraForward;

            sightColliders.Clear();
            numRaycasts = 0;

            Profiler.BeginSample($"{nameof(SightSense)}.AddRaycastCommands");
            foreach (var collider in values)
            {
                if (IsWithinFov(cameraPosition, cameraForward, collider.transform.position))
                {
                    var relPosToItem = collider.bounds.center - cameraPosition;

                    raycastCommandsBuffer[numRaycasts] = new RaycastCommand(cameraPosition, relPosToItem, relPosToItem.magnitude, ~excludedCollisionLayerMask, RaycastMaxHits);
                    numRaycasts++;

                    sightColliders.Add(collider);
                }
            }
            Profiler.EndSample();

            var raycastCommands = raycastCommandsBuffer.GetSubArray(0, numRaycasts);

            var raycastsJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResultsBuffer, numRaycasts / JobsUtility.JobWorkerCount);
            raycastsJobHandle.Complete();

            Profiler.BeginSample($"{nameof(SightSense)}.ProcessRaycastResults");
            var withinSights = new List<Collider>(numRaycasts);
            for (int i = 0; i < numRaycasts; i++)
            {
                var numHits = 0;

                var raycastHitsStart = i * RaycastMaxHits;
                for (int j = 0; j < RaycastMaxHits; j++)
                {
                    var hit = raycastResultsBuffer[raycastHitsStart + j];
                    if (hit.collider == null)
                    {
                        break;
                    }

                    numHits++;
                }

                if (numHits > 0)
                {
                    RaycastHit? hit = null;
                    for (int j = 0; j < numHits; j++)
                    {
                        var h = raycastResultsBuffer[raycastHitsStart + j];
                        if (h.collider.GetComponentInParent<ReferenceHub>() is not ReferenceHub otherHub || otherHub != playerHub)
                        {
                            hit = h;
                            break;
                        }
                    }

                    if (hit.HasValue && sightColliders[i] == hit.Value.collider)
                    {
                        withinSights.Add(hit.Value.collider);
                    }
                }
            }
            Profiler.EndSample();

            return withinSights;
        }

        protected bool IsWithinSight<T>(Collider collider, T item) where T : Component
        {
            var playerHub = _fpcBotPlayer.BotHub.PlayerHub;

            var cameraPosition = _fpcBotPlayer.CameraPosition;
            var cameraForward = _fpcBotPlayer.CameraForward;

            if (IsWithinFov(cameraPosition, cameraForward, collider.transform.position))
            {
                var relPosToItem = collider.bounds.center - cameraPosition;
                _numHits = Physics.RaycastNonAlloc(cameraPosition, relPosToItem, _hitsBuffer, relPosToItem.magnitude, ~excludedCollisionLayerMask);

                if (_numHits == HitsBufferSize)
                {
                    Log.Warning($"{nameof(SightSense)} num of hits equal to buffer size, possible cuts.");
                }

                if (_numHits > 0)
                {
                    Array.Sort(_hitsBuffer, 0, _numHits, distanceComparer);

                    RaycastHit? hit = null;
                    for (int i = 0; i < _numHits; i++)
                    {
                        var h = _hitsBuffer[i];
                        if (h.collider.GetComponentInParent<ReferenceHub>() is not ReferenceHub otherHub || otherHub != playerHub)
                        {
                            hit = h;
                            break;
                        }
                    }

                    if (hit.HasValue && hit.Value.collider.GetComponentInParent<T>() is T hitItem
                        && hitItem == item)
                    {
                        return true;
                    }
                }
            }

            return false;
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
