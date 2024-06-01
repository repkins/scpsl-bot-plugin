using Interactables;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class PlayersWithinSightSense : SightSense
    {
        public HashSet<ReferenceHub> PlayersWithinSight { get; } = new HashSet<ReferenceHub>();
        public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }
        public IEnumerable<ReferenceHub> FriendiesWithinSight { get; }

        public PlayersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;

            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != botPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == botPlayer.BotHub.PlayerHub.GetFaction());
        }

        public override void Reset()
        {
            PlayersWithinSight.Clear();
        }

        private static readonly Dictionary<Collider, ReferenceHub> allCollidersToComponent = new();

        private Dictionary<Collider, ReferenceHub> validCollidersToComponent = new();

        public override IEnumerator<JobHandle> ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(PlayersWithinSightSense)}.{nameof(ProcessSensibility)}");

            validCollidersToComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((hitboxLayerMask & (1 << collider.gameObject.layer)) != 0)
                {
                    if (!allCollidersToComponent.TryGetValue(collider, out var otherPlayer))
                    {
                        otherPlayer = collider.GetComponentInParent<ReferenceHub>();
                        allCollidersToComponent.Add(collider, otherPlayer);
                    }

                    if (otherPlayer != null && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub)
                    {
                        validCollidersToComponent.Add(collider, otherPlayer);
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
                PlayersWithinSight.Add(validCollidersToComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask hitboxLayerMask = LayerMask.GetMask("Hitbox");

        public override void ProcessSightSensedItems() { }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
