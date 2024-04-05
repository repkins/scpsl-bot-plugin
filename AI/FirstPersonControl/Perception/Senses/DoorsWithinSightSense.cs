using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class DoorsWithinSightSense : SightSense, ISense
    {
        public HashSet<DoorVariant> DoorsWithinSight { get; } = new();

        public event Action OnBeforeSensedDoorsWithinSight;
        public event Action<DoorVariant> OnSensedDoorWithinSight;
        public event Action OnAfterSensedDoorsWithinSight;

        public DoorsWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void Reset()
        {
            DoorsWithinSight.Clear();
        }

        public void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<DoorVariant>() is DoorVariant door
                && !DoorsWithinSight.Contains(door))
            {
                if (IsWithinSight(collider, door))
                {
                    DoorsWithinSight.Add(door);
                }
            }
        }

        public void ProcessSensedItems()
        {
            OnBeforeSensedDoorsWithinSight?.Invoke();
            foreach (var doorWithinSight in DoorsWithinSight)
            {
                OnSensedDoorWithinSight?.Invoke(doorWithinSight);
            }
            OnAfterSensedDoorsWithinSight?.Invoke();
        }

        public IEnumerable<DoorVariant> GetDoorsOnPath(IEnumerable<Vector3> pathOfPoints)
        {
            var rays = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

            var doorsOnPath = rays
                .Select(ray => DoorsWithinSight
                    .FirstOrDefault(door => door.GetComponentsInChildren<Collider>()
                        .Any(collider => collider.Raycast(ray, out _, 1f))))
                .Where(d => d != null);

            return doorsOnPath;
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
