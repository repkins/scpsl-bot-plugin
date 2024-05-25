using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class LockersWithinSightSense : SightSense
    {
        public HashSet<Locker> LockersWithinSight { get; } = new();

        public LockersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public override void Reset()
        {
            LockersWithinSight.Clear();
        }

        private static Dictionary<Collider, Locker> allCollidersToComponent = new();

        private Dictionary<Collider, Locker> validCollidersToComponent = new();

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(LockersWithinSightSense)}.{nameof(ProcessSensibility)}");

            validCollidersToComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((interactableLayerMask & (1 << collider.gameObject.layer)) != 0)
                {
                    if (!allCollidersToComponent.TryGetValue(collider, out var locker))
                    {
                        locker = collider.GetComponentInParent<Locker>();

                        allCollidersToComponent.Add(collider, locker);
                    }

                    if (locker != null)
                    {
                        validCollidersToComponent.Add(collider, locker);
                    }
                }
            }


            var withinSight = new List<Collider>();
            var withinSightHandles = this.GetWithinSight(validCollidersToComponent.Keys, withinSight);
            while (withinSightHandles.MoveNext())
            {
                yield return withinSightHandles.Current;
            }


            foreach (var collider in withinSight)
            {
                LockersWithinSight.Add(validCollidersToComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");

        public override void ProcessSightSensedItems()
        {
            //var numLockers = 0u;
            //var lockerWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<LockerWithinSight<Locker>>();
            //foreach (var lockerWithinSight in LockersWithinSight)
            //{
            //    if (lockerWithinSight is Locker locker)
            //    {
            //        if (lockerWithinSightBelief.Locker is null)
            //        {
            //            UpdateLockerBelief(lockerWithinSightBelief, locker);
            //        }
            //        numLockers++;
            //    }
            //}
            //if (numLockers <= 0 && lockerWithinSightBelief.Locker is not null)
            //{
            //    UpdateLockerBelief(lockerWithinSightBelief, null as Locker);
            //}
        }

        //private static void UpdateLockerBelief<T, I>(T lockerBelief, I locker) where T : LockerBase<I> where I : Locker
        //{
        //    lockerBelief.Update(locker);
        //    Log.Debug($"{lockerBelief.GetType().Name} updated: {locker}");
        //}

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
