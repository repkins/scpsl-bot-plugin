using InventorySystem.Items.Firearms;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal class FpcBotPerception
    {
        public HashSet<ReferenceHub> PlayersWithinSight { get; } = new HashSet<ReferenceHub>();
        public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }
        public IEnumerable<ReferenceHub> FriendiesWithinSight { get; }

        public bool HasFirearmInInventory { get; private set; }

        public FpcBotPerception(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != fpcBotPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == fpcBotPlayer.BotHub.PlayerHub.GetFaction());
        }

        public void Tick(IFpcRole fpcRole)
        {
            var fpcTransform = fpcRole.FpcModule.transform;

            var prevNumOverlappingColliders = _numOverlappingColliders;
            _numOverlappingColliders = Physics.OverlapSphereNonAlloc(fpcTransform.position, 10f, _overlappingCollidersBuffer);

            if (_numOverlappingColliders >= OverlappingCollidersBufferSize && _numOverlappingColliders != prevNumOverlappingColliders)
            {
                Log.Warning($"Num of overlapping colliders is equal to it's buffer size. Possible cuts.");
            }

            var overlappingColliders = _overlappingCollidersBuffer.Take(_numOverlappingColliders);

            PlayersWithinSight.Clear();

            var facingDir = fpcTransform.forward;
            foreach (var collider in overlappingColliders)
            {
                if (collider.GetComponentInParent<ReferenceHub>() is ReferenceHub otherPlayer
                    && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub
                    && !PlayersWithinSight.Contains(otherPlayer))
                {
                    if (IsWithinFov(fpcTransform, facingDir, collider.transform)
                        && Physics.Raycast(fpcTransform.position, otherPlayer.transform.position - fpcTransform.position, out var hit)
                        && hit.collider.GetComponentInParent<ReferenceHub>() is ReferenceHub hitHub
                        && hitHub == otherPlayer)
                    {
                        PlayersWithinSight.Add(otherPlayer);
                    }
                }
            }

            HasFirearmInInventory = _fpcBotPlayer.BotHub.PlayerHub.inventory.UserInventory.Items.Any(i => i.Value is Firearm);
        }

        private bool IsWithinFov(Transform transform, Vector3 facingDir, Transform targetTransform)
        {
            var diff = targetTransform.position - transform.position;

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

        private const int OverlappingCollidersBufferSize = 1000;

        private static int _numOverlappingColliders;
        private static Collider[] _overlappingCollidersBuffer = new Collider[OverlappingCollidersBufferSize];

        private FpcBotPlayer _fpcBotPlayer;
    }
}
