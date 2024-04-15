using Interactables.Interobjects.DoorUtils;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

            Senses.Add(new LockersWithinSightSense(fpcBotPlayer));
            
            Senses.Add(new SpatialSense(fpcBotPlayer));

            Senses.Add(new RoomSightSense(fpcBotPlayer));

            Senses.Add(new InteractablesWithinSightSense(fpcBotPlayer));
        }

        public void Tick(IFpcRole fpcRole)
        {
            //var fpcTransform = fpcRole.FpcModule.transform;
            var cameraTransform = _fpcBotPlayer.BotHub.PlayerHub.PlayerCameraReference;

            var prevNumOverlappingColliders = _numOverlappingColliders;
            _numOverlappingColliders = Physics.OverlapSphereNonAlloc(cameraTransform.position, 32f, _overlappingCollidersBuffer, _perceptionLayerMask);

            if (_numOverlappingColliders >= OverlappingCollidersBufferSize && _numOverlappingColliders != prevNumOverlappingColliders)
            {
                Log.Warning($"Num of overlapping colliders is equal to it's buffer size. Possible cuts.");
            }

            var overlappingColliders = _overlappingCollidersBuffer.Take(_numOverlappingColliders);

            foreach (var sense in Senses)
            {
                sense.Reset();
            }

            foreach (var collider in overlappingColliders)
            {
                foreach (var sense in Senses)
                {
                    sense.ProcessSensibility(collider);
                }
            }

            foreach (var sense in Senses)
            {
                sense.ProcessSensedItems();
            }
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
