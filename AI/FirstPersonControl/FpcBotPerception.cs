using Interactables.Interobjects.DoorUtils;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotPerception
    {
        public List<ISense> Senses { get; } = new();
        public DoorsWithinSightSense DoorsSense { get; private set; }
        public PlayersWithinSightSense PlayersSense { get; private set; }
        public ItemsInInventorySense InventorySense { get; private set; }

        #region Debugging
        public Dictionary<Collider, (int, string)> Layers { get; } = new Dictionary<Collider, (int, string)>();
        #endregion

        public FpcBotPerception(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;

            Senses.Add(new ItemsWithinSightSense(fpcBotPlayer));

            DoorsSense = new DoorsWithinSightSense(fpcBotPlayer);
            Senses.Add(DoorsSense);

            PlayersSense = new PlayersWithinSightSense(fpcBotPlayer);
            Senses.Add(PlayersSense);

            InventorySense = new ItemsInInventorySense(fpcBotPlayer);
            Senses.Add(InventorySense);

            Senses.Add(new GlassSightSense(fpcBotPlayer));

            Senses.Add(new LockersWithinSightSense(fpcBotPlayer));
            
            Senses.Add(new SpatialSense(fpcBotPlayer));

            Senses.Add(new RoomSightSense(fpcBotPlayer));

            Senses.Add(new InteractablesWithinSightSense(fpcBotPlayer));
        }

        private readonly List<IEnumerator<JobHandle>> processSensesEnumerators = new();

        public IEnumerator<JobHandle> Update()
        {
            Profiler.BeginSample($"{nameof(FpcBotPerception)}.{nameof(Update)}");

            var sensesCount = Senses.Count;

            processSensesEnumerators.Clear();
            foreach (var sense in Senses)
            {
                processSensesEnumerators.Add(sense.ProcessSensibility());
            }

            var completedCount = 0;
            var jobHandlesCount = 0;
            var jobHandlesBuffer = new NativeArray<JobHandle>(sensesCount, Allocator.Temp);
            while (completedCount < sensesCount)
            {
                completedCount = 0;
                jobHandlesCount = 0;
                for (int i = 0; i < sensesCount; i++)
                {
                    var processSenses = processSensesEnumerators[i];
                    if (processSenses.MoveNext())
                    {
                        jobHandlesBuffer[jobHandlesCount] = processSenses.Current;
                        jobHandlesCount++;
                    }
                    else
                    {
                        completedCount++;
                    }
                }

                var jobHandles = jobHandlesBuffer.GetSubArray(0, jobHandlesCount);
                yield return JobHandle.CombineDependencies(jobHandles);
            }
            jobHandlesBuffer.Dispose();

            foreach (var sense in Senses)
            {
                sense.ProcessSensedItems();
            }

            Profiler.EndSample();
        }

        public IEnumerable<DoorVariant> GetDoorsOnPath(IEnumerable<Vector3> pathOfPoints)
        {
            var rays = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

            var doorsOnPath = rays
                .Select(ray => DoorsSense.DoorsWithinSight
                    .FirstOrDefault(door => door.GetComponentsInChildren<Collider>()
                        .Any(collider => collider.Raycast(ray, out _, 1f))))
                .Where(d => d != null);

            return doorsOnPath;
        }

        public T GetSense<T>() where T : class
        {
            return Senses.Find(s => s is T) as T;
        }

        private const int OverlappingCollidersBufferSize = 1000;
        private static readonly Collider[] _overlappingCollidersBuffer = new Collider[OverlappingCollidersBufferSize];
        private static int _numOverlappingColliders;

        private FpcBotPlayer _fpcBotPlayer;

        private LayerMask _perceptionLayerMask = LayerMask.GetMask("Hitbox", "Door", "InteractableNoPlayerCollision", "Glass");
    }
}
