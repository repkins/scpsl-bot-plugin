using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class LockersWithinSightSense : SightSense<Locker>
    {
        public HashSet<Locker> LockersWithinSight => ComponentsWithinSight;

        public LockersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        { }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");
        protected override LayerMask LayerMask => interactableLayerMask;

        private readonly List<InteractableCollider> interactables = new();

        protected override Locker GetComponent(ref Collider colliderToBeChecked)
        {
            var lockerChamber = colliderToBeChecked.GetComponentInParent<LockerChamber>();
            var locker = lockerChamber?.GetComponentInParent<Locker>();

            if (locker != null)
            {
                locker.GetComponentsInChildren(interactables);

                var colliderId = Array.IndexOf(locker.Chambers, lockerChamber);
                colliderId %= interactables.Count;

                foreach (var interactable in interactables)
                {
                    if (interactable.ColliderId == colliderId)
                    {
                        colliderToBeChecked = interactable.GetComponentInChildren<Collider>();
                        break;
                    }
                }
            }

            return locker;
        }

        public override void ProcessSightSensedItems()
        {
        }
    }
}
