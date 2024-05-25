using Interactables;
using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class InteractablesWithinSightSense : SightSense
    {
        public HashSet<InteractableCollider> InteractableCollidersWithinSight { get; } = new();

        public event Action OnBeforeSensedInteractablesWithinSight;
        public event Action<InteractableCollider> OnSensedInteractableColliderWithinSight;
        public event Action OnAfterSensedInteractablesWithinSight;

        public InteractablesWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }

        public override void Reset()
        {
            InteractableCollidersWithinSight.Clear();
        }

        private static Dictionary<Collider, InteractableCollider> allCollidersToComponent = new();

        private Dictionary<Collider, InteractableCollider> validCollidersToComponent = new();

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(InteractablesWithinSightSense)}.{nameof(ProcessSensibility)}");

            validCollidersToComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((interactableLayerMask & (1 << collider.gameObject.layer)) != 0)
                {
                    if (!allCollidersToComponent.TryGetValue(collider, out var interactable))
                    {
                        interactable = collider.GetComponentInParent<InteractableCollider>();
                        allCollidersToComponent.Add(collider, interactable);
                    }

                    if (interactable != null)
                    {
                        validCollidersToComponent.Add(collider, interactable);
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
                InteractableCollidersWithinSight.Add(validCollidersToComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");

        public override void ProcessSightSensedItems()
        {
            OnBeforeSensedInteractablesWithinSight?.Invoke();
            foreach (var interactableWithinSight in InteractableCollidersWithinSight)
            {
                OnSensedInteractableColliderWithinSight?.Invoke(interactableWithinSight);
            }
            OnAfterSensedInteractablesWithinSight?.Invoke();
        }
    }
}
